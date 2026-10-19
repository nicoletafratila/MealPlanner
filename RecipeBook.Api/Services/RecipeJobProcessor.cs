using Common.Pagination;
using Newtonsoft.Json;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Services
{
    public class RecipeJobProcessor(
        IJobRepository jobRepository,
        IRecipeRepository recipeRepository,
        IProductRepository productRepository,
        IRecipeCategoryShareResolver recipeCategoryResolver,
        IProductCategoryShareResolver productCategoryResolver,
        IRecipeShareProgressNotifier notifier,
        ILogger<RecipeJobProcessor> logger) : IRecipeJobProcessor
    {
        private const int MaxNameAttempts = 50;

        private readonly IJobRepository _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private readonly IRecipeCategoryShareResolver _recipeCategoryResolver = recipeCategoryResolver ?? throw new ArgumentNullException(nameof(recipeCategoryResolver));
        private readonly IProductCategoryShareResolver _productCategoryResolver = productCategoryResolver ?? throw new ArgumentNullException(nameof(productCategoryResolver));
        private readonly IRecipeShareProgressNotifier _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        private readonly ILogger<RecipeJobProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
            if (job is null)
                return;

            try
            {
                job.Status = JobStatus.Running;
                await _jobRepository.UpdateAsync(job, cancellationToken);
                await _notifier.NotifyAsync(job, cancellationToken);

                var filters = string.IsNullOrWhiteSpace(job.FiltersJson)
                    ? null
                    : JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(job.FiltersJson);

                var sourceRecipes = await _recipeRepository.GetAllByUserIncludeIngredientsAsync(job.OwnerUserId!, filters, cancellationToken);

                job.TotalCount = sourceRecipes.Count;
                await _jobRepository.UpdateAsync(job, cancellationToken);
                await _notifier.NotifyAsync(job, cancellationToken);

                if (sourceRecipes.Count > 0)
                {
                    var recipeCategoryMap = await _recipeCategoryResolver.ResolveAsync(
                        sourceRecipes.Select(r => r.RecipeCategory),
                        job.TargetUserId!,
                        cancellationToken);

                    var productCategoryMap = await _productCategoryResolver.ResolveAsync(
                        sourceRecipes.SelectMany(r => r.RecipeIngredients ?? []).Select(ri => ri.Product?.ProductCategory),
                        job.TargetUserId!,
                        cancellationToken);

                    var targetProductIdsBySourceProductId = await CloneProductsAsync(sourceRecipes, productCategoryMap, job.TargetUserId!, cancellationToken);
                    var sharedRecipes = await BuildSharedRecipesAsync(
                        job,
                        sourceRecipes,
                        targetProductIdsBySourceProductId,
                        recipeCategoryMap,
                        cancellationToken);

                    await _recipeRepository.AddRangeAsync(sharedRecipes, cancellationToken);
                }

                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job, cancellationToken);
                await _notifier.NotifyAsync(job, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when processing share job {JobId}.", jobId);

                job.Status = JobStatus.Failed;
                job.ErrorMessage = RecipeMessages.SaveFailed;
                job.CompletedAt = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job, cancellationToken);
                await _notifier.NotifyAsync(job, cancellationToken);
            }
        }

        private async Task<Dictionary<Guid, Guid>> CloneProductsAsync(
            IReadOnlyList<Recipe> sourceRecipes,
            Dictionary<Guid, Guid> productCategoryMap,
            string targetUserId,
            CancellationToken cancellationToken)
        {
            var existingTargetProducts = await _productRepository.GetAllByUserAsync(targetUserId, cancellationToken);
            var existingTargetProductsByNameAndCategory = existingTargetProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .GroupBy(p => (p.ProductCategoryId, Name: p.Name!.ToLowerInvariant()))
                .ToDictionary(g => g.Key, g => g.First());

            var targetProductBySourceProductId = new Dictionary<Guid, Product>();
            var stagedProductsByNameAndCategory = new Dictionary<(Guid ProductCategoryId, string Name), Product>();
            var newProducts = new List<Product>();

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

                var cloned = new Product
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

        private async Task<List<Recipe>> BuildSharedRecipesAsync(
            Job job,
            IReadOnlyList<Recipe> sourceRecipes,
            Dictionary<Guid, Guid> targetProductIdsBySourceProductId,
            Dictionary<Guid, Guid> recipeCategoryMap,
            CancellationToken cancellationToken)
        {
            var existingTargetRecipes = await _recipeRepository.GetAllByUserAsync(job.TargetUserId!, cancellationToken);
            var usedNamesByCategory = existingTargetRecipes
                .Where(r => r.Name is not null)
                .GroupBy(r => r.RecipeCategoryId)
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<string>(g.Select(r => r.Name!), StringComparer.OrdinalIgnoreCase));

            var sharedRecipes = new List<Recipe>();

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
                    .Select(ri => new RecipeIngredient
                    {
                        ProductId = targetProductIdsBySourceProductId[ri.ProductId],
                        UnitId = ri.UnitId,
                        Quantity = ri.Quantity
                    })
                    .ToList();

                sharedRecipes.Add(new Recipe
                {
                    Name = uniqueName,
                    Source = source.Source,
                    ImageContent = source.ImageContent,
                    ImageThumbnail = source.ImageThumbnail,
                    RecipeCategoryId = targetCategoryId,
                    UserId = job.TargetUserId,
                    RecipeIngredients = sharedIngredients
                });

                job.ProcessedCount++;
                await _jobRepository.UpdateAsync(job, cancellationToken);
                await _notifier.NotifyAsync(job, cancellationToken);
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
