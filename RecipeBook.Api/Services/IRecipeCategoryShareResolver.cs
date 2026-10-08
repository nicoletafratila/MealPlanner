using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Resolves the best-matching (or newly created) <see cref="RecipeCategory"/> for a target user when sharing recipes.
    /// </summary>
    public interface IRecipeCategoryShareResolver
    {
        /// <summary>
        /// For each distinct source category, finds a same-named category already owned by the target user, or creates
        /// one. Returns a map of source category id to resolved target category id.
        /// </summary>
        Task<Dictionary<Guid, Guid>> ResolveAsync(
            IEnumerable<RecipeCategory?> sourceCategories,
            string targetUserId,
            CancellationToken cancellationToken);
    }
}
