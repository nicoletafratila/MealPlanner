using Common.Models;
using Common.Services;
using MediatR;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    /// <summary>
    /// Handles sharing all of the current user's recipes with another user by cloning them, and the products they reference,
    /// in a small number of batched database round trips instead of one per recipe.
    /// </summary>
    public class ShareAllCommandHandler(
        IRecipeRepository repository,
        IProductRepository productRepository,
        IRecipeCategoryShareResolver recipeCategoryResolver,
        IProductCategoryShareResolver productCategoryResolver,
        ICurrentUserService currentUserService,
        ILogger<ShareAllCommandHandler> logger) : IRequestHandler<ShareAllCommand, CommandResponse?>
    {
        private const int MaxNameAttempts = 50;

        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private readonly IRecipeCategoryShareResolver _recipeCategoryResolver = recipeCategoryResolver ?? throw new ArgumentNullException(nameof(recipeCategoryResolver));
        private readonly IProductCategoryShareResolver _productCategoryResolver = productCategoryResolver ?? throw new ArgumentNullException(nameof(productCategoryResolver));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<ShareAllCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareAllCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(RecipeMessages.UserIdRequired);

                var sourceRecipes = await _repository.GetAllByUserIncludeIngredientsAsync(userId, request.Filters, cancellationToken);
                if (sourceRecipes.Count == 0)
                    return CommandResponse.Success();

                var recipeCategoryMap = await _recipeCategoryResolver.ResolveAsync(
                    sourceRecipes.Select(r => r.RecipeCategory),
                    request.TargetUserId,
                    cancellationToken);

                var productCategoryMap = await _productCategoryResolver.ResolveAsync(
                    sourceRecipes.SelectMany(r => r.RecipeIngredients ?? []).Select(ri => ri.Product?.ProductCategory),
                    request.TargetUserId,
                    cancellationToken);

                var targetProductIdsBySourceProductId = await CloneProductsAsync(sourceRecipes, productCategoryMap, request.TargetUserId, cancellationToken);
                var sharedRecipes = await BuildSharedRecipesAsync(sourceRecipes, targetProductIdsBySourceProductId, recipeCategoryMap, request.TargetUserId, cancellationToken);

                await _repository.AddRangeAsync(sharedRecipes, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing all recipes to user {TargetUserId}.", request.TargetUserId);
                return CommandResponse.Failed(RecipeMessages.SaveFailed);
            }
        }

        private async Task<Dictionary<Guid, Guid>> CloneProductsAsync(
            IReadOnlyList<Data.Entities.Recipe> sourceRecipes,
            Dictionary<Guid, Guid> productCategoryMap,
            string targetUserId,
            CancellationToken cancellationToken)
        {
            var existingTargetProducts = await _productRepository.GetAllByUserAsync(targetUserId, cancellationToken);
            var existingTargetProductsByNameAndCategory = existingTargetProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .GroupBy(p => (p.ProductCategoryId, Name: p.Name!.ToLowerInvariant()))
                .ToDictionary(g => g.Key, g => g.First());

            var targetProductBySourceProductId = new Dictionary<Guid, Data.Entities.Product>();
            var stagedProductsByNameAndCategory = new Dictionary<(Guid ProductCategoryId, string Name), Data.Entities.Product>();
            var newProducts = new List<Data.Entities.Product>();

            foreach (var ingredient in sourceRecipes.SelectMany(r => r.RecipeIngredients ?? []))
            {
                if (targetProductBySourceProductId.ContainsKey(ingredient.ProductId))
                    continue;

                var product = ingredient.Product;
                ArgumentNullException.ThrowIfNull(product);

                var targetCategoryId = productCategoryMap[product.ProductCategoryId];
                var hasName = !string.IsNullOrWhiteSpace(product.Name);
                var key = (targetCategoryId, Name: product.Name?.ToLowerInvariant() ?? string.Empty);

                if (hasName && existingTargetProductsByNameAndCategory.TryGetValue(key, out var existing))
                {
                    targetProductBySourceProductId[ingredient.ProductId] = existing;
                    continue;
                }

                if (hasName && stagedProductsByNameAndCategory.TryGetValue(key, out var staged))
                {
                    targetProductBySourceProductId[ingredient.ProductId] = staged;
                    continue;
                }

                var cloned = new Data.Entities.Product
                {
                    Name = product.Name,
                    ImageContent = product.ImageContent,
                    ImageThumbnail = product.ImageThumbnail,
                    BaseUnitId = product.BaseUnitId,
                    ProductCategoryId = targetCategoryId,
                    UserId = targetUserId
                };

                newProducts.Add(cloned);
                targetProductBySourceProductId[ingredient.ProductId] = cloned;

                if (hasName)
                    stagedProductsByNameAndCategory[key] = cloned;
            }

            if (newProducts.Count > 0)
                await _productRepository.AddRangeAsync(newProducts, cancellationToken);

            return targetProductBySourceProductId.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id);
        }

        private async Task<List<Data.Entities.Recipe>> BuildSharedRecipesAsync(
            IReadOnlyList<Data.Entities.Recipe> sourceRecipes,
            Dictionary<Guid, Guid> targetProductIdsBySourceProductId,
            Dictionary<Guid, Guid> recipeCategoryMap,
            string targetUserId,
            CancellationToken cancellationToken)
        {
            var existingTargetRecipes = await _repository.GetAllByUserAsync(targetUserId, cancellationToken);
            var usedNamesByCategory = existingTargetRecipes
                .Where(r => r.Name is not null)
                .GroupBy(r => r.RecipeCategoryId)
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<string>(g.Select(r => r.Name!), StringComparer.OrdinalIgnoreCase));

            var sharedRecipes = new List<Data.Entities.Recipe>();

            foreach (var source in sourceRecipes)
            {
                var targetCategoryId = recipeCategoryMap[source.RecipeCategoryId];
                if (!usedNamesByCategory.TryGetValue(targetCategoryId, out var usedNames))
                {
                    usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    usedNamesByCategory[targetCategoryId] = usedNames;
                }

                var uniqueName = ResolveUniqueRecipeName(source.Name, usedNames);

                var sharedIngredients = source.RecipeIngredients?
                    .Select(ri => new Data.Entities.RecipeIngredient
                    {
                        ProductId = targetProductIdsBySourceProductId[ri.ProductId],
                        UnitId = ri.UnitId,
                        Quantity = ri.Quantity
                    })
                    .ToList();

                sharedRecipes.Add(new Data.Entities.Recipe
                {
                    Name = uniqueName,
                    Source = source.Source,
                    ImageContent = source.ImageContent,
                    ImageThumbnail = source.ImageThumbnail,
                    RecipeCategoryId = targetCategoryId,
                    UserId = targetUserId,
                    RecipeIngredients = sharedIngredients
                });
            }

            return sharedRecipes;
        }

        private static string? ResolveUniqueRecipeName(string? name, HashSet<string> usedNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var candidate = name;
            for (var attempt = 1; attempt <= MaxNameAttempts; attempt++)
            {
                if (usedNames.Add(candidate))
                    return candidate;

                candidate = attempt == 1 ? $"{name} (Copy)" : $"{name} (Copy {attempt})";
            }

            usedNames.Add(candidate);
            return candidate;
        }
    }
}
