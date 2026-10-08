using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Resolves the best-matching (or newly created) <see cref="RecipeCategory"/> for a target user when sharing recipes.
    /// </summary>
    public class RecipeCategoryShareResolver(IRecipeCategoryRepository repository) : IRecipeCategoryShareResolver
    {
        private readonly IRecipeCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<Dictionary<Guid, Guid>> ResolveAsync(
            IEnumerable<RecipeCategory?> sourceCategories,
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

            var nextDisplaySequence = targetCategories.Count == 0
                ? 0
                : targetCategories.Max(c => c.DisplaySequence) + 1;

            var targetEntityBySourceId = new Dictionary<Guid, RecipeCategory>();
            var newCategoriesByName = new Dictionary<string, RecipeCategory>(StringComparer.OrdinalIgnoreCase);
            var newCategories = new List<RecipeCategory>();

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

                var created = new RecipeCategory
                {
                    Name = source.Name,
                    UserId = targetUserId,
                    DisplaySequence = nextDisplaySequence++
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
