using Common.Data.Repository;
using Common.Pagination;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and updating <see cref="RecipeCategory"/> entities.
    /// </summary>
    public interface IRecipeCategoryRepository : IAsyncRepository<RecipeCategory, Guid>
    {
        Task<IReadOnlyList<RecipeCategory>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<RecipeCategory>> GetByIdsAsync(IList<Guid> ids, CancellationToken cancellationToken);

        /// <summary>
        /// Updates all provided recipe categories in a single save operation.
        /// </summary>
        Task UpdateAllAsync(IList<RecipeCategory> entities, CancellationToken cancellationToken);

        /// <summary>
        /// Filters and sorts recipe categories at the database level, optionally restricted to the given ids.
        /// An empty <paramref name="ids"/> list matches all categories.
        /// </summary>
        Task<IReadOnlyList<RecipeCategory>> SearchByIdsAsync(
            IList<Guid> ids,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            CancellationToken cancellationToken);

        /// <summary>
        /// Filters, sorts, and pages recipe categories for a user at the database level, returning only the requested
        /// page. Defaults to DisplaySequence order when no explicit sorting is requested.
        /// </summary>
        Task<PagedQueryResult<RecipeCategory>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}