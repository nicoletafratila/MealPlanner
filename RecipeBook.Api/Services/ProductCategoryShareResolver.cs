using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Resolves the best-matching (or newly created) <see cref="ProductCategory"/> for a target user when sharing products.
    /// </summary>
    public class ProductCategoryShareResolver(IProductCategoryRepository repository) : IProductCategoryShareResolver
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<Dictionary<Guid, Guid>> ResolveAsync(
            IEnumerable<ProductCategory?> sourceCategories,
            string targetUserId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sourceCategories);

            var distinctSourceCategories = sourceCategories
                .Where(c => c is not null)
                .Select(c => c!)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            var map = new Dictionary<Guid, Guid>();
            if (distinctSourceCategories.Count == 0)
                return map;

            var targetCategories = await _repository.GetAllByUserAsync(targetUserId, cancellationToken);
            var targetByName = targetCategories
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .GroupBy(c => c.Name!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var targetEntityBySourceId = new Dictionary<Guid, ProductCategory>();
            var newCategoriesByName = new Dictionary<string, ProductCategory>(StringComparer.OrdinalIgnoreCase);
            var newCategories = new List<ProductCategory>();

            foreach (var source in distinctSourceCategories)
            {
                if (!string.IsNullOrWhiteSpace(source.Name) && targetByName.TryGetValue(source.Name, out var existing))
                {
                    targetEntityBySourceId[source.Id] = existing;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(source.Name) && newCategoriesByName.TryGetValue(source.Name, out var staged))
                {
                    targetEntityBySourceId[source.Id] = staged;
                    continue;
                }

                var created = new ProductCategory
                {
                    Name = source.Name,
                    UserId = targetUserId
                };

                newCategories.Add(created);
                targetEntityBySourceId[source.Id] = created;

                if (!string.IsNullOrWhiteSpace(source.Name))
                    newCategoriesByName[source.Name] = created;
            }

            if (newCategories.Count > 0)
                await _repository.AddRangeAsync(newCategories, cancellationToken);

            foreach (var (sourceId, targetEntity) in targetEntityBySourceId)
                map[sourceId] = targetEntity.Id;

            return map;
        }
    }
}
