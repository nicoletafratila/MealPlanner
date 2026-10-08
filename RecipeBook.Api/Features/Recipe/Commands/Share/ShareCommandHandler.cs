using Common.Models;
using MediatR;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Share
{
    /// <summary>
    /// Handles sharing a recipe by cloning it, and the products it references, into new records owned by another user.
    /// </summary>
    public class ShareCommandHandler(
        IRecipeRepository repository,
        IProductRepository productRepository,
        ILogger<ShareCommandHandler> logger) : IRequestHandler<ShareCommand, CommandResponse?>
    {
        private const int MaxNameAttempts = 50;

        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private readonly ILogger<ShareCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var source = await _repository.GetByIdIncludeIngredientsAsync(request.RecipeId, cancellationToken);
                if (source is null)
                {
                    return CommandResponse.Failed(string.Format(RecipeMessages.NotFoundById, request.RecipeId));
                }

                var uniqueName = await ResolveUniqueRecipeNameAsync(source.Name, request.TargetUserId, cancellationToken);
                var sharedIngredients = await ShareProductsAsync(source.RecipeIngredients, request.TargetUserId, cancellationToken);

                var shared = new Data.Entities.Recipe
                {
                    Name = uniqueName,
                    Source = source.Source,
                    ImageContent = source.ImageContent,
                    ImageThumbnail = source.ImageThumbnail,
                    RecipeCategoryId = source.RecipeCategoryId,
                    UserId = request.TargetUserId,
                    RecipeIngredients = sharedIngredients
                };

                await _repository.AddAsync(shared, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing the recipe with id {RecipeId} to user {TargetUserId}.", request.RecipeId, request.TargetUserId);
                return CommandResponse.Failed(RecipeMessages.SaveFailed);
            }
        }

        private async Task<List<Data.Entities.RecipeIngredient>?> ShareProductsAsync(
            IEnumerable<Data.Entities.RecipeIngredient>? ingredients,
            string targetUserId,
            CancellationToken cancellationToken)
        {
            if (ingredients is null)
                return null;

            var clonedProductIds = new Dictionary<Guid, Guid>();
            var sharedIngredients = new List<Data.Entities.RecipeIngredient>();

            foreach (var ingredient in ingredients)
            {
                if (!clonedProductIds.TryGetValue(ingredient.ProductId, out var clonedProductId))
                {
                    clonedProductId = await CloneProductAsync(ingredient.Product, targetUserId, cancellationToken);
                    clonedProductIds[ingredient.ProductId] = clonedProductId;
                }

                sharedIngredients.Add(new Data.Entities.RecipeIngredient
                {
                    ProductId = clonedProductId,
                    UnitId = ingredient.UnitId,
                    Quantity = ingredient.Quantity
                });
            }

            return sharedIngredients;
        }

        private async Task<Guid> CloneProductAsync(
            Data.Entities.Product? product,
            string targetUserId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (!string.IsNullOrWhiteSpace(product.Name))
            {
                var existing = await _productRepository.SearchAsync(product.Name, targetUserId, cancellationToken);
                if (existing is not null)
                    return existing.Id;
            }

            var cloned = new Data.Entities.Product
            {
                Name = product.Name,
                ImageContent = product.ImageContent,
                ImageThumbnail = product.ImageThumbnail,
                BaseUnitId = product.BaseUnitId,
                ProductCategoryId = product.ProductCategoryId,
                UserId = targetUserId
            };

            var added = await _productRepository.AddAsync(cloned, cancellationToken);
            return added.Id;
        }

        private async Task<string?> ResolveUniqueRecipeNameAsync(string? name, string targetUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var candidate = name;
            for (var attempt = 1; attempt <= MaxNameAttempts; attempt++)
            {
                var existing = await _repository.SearchAsync(candidate, targetUserId, cancellationToken);
                if (existing is null)
                    return candidate;

                candidate = attempt == 1 ? $"{name} (Copy)" : $"{name} (Copy {attempt})";
            }

            return candidate;
        }
    }
}
