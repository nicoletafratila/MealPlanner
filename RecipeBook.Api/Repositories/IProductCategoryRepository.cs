using Common.Data.Repository;
using Common.Pagination;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="ProductCategory"/> entities.
    /// </summary>
    public interface IProductCategoryRepository : IAsyncRepository<ProductCategory, Guid>
    {
        Task<IReadOnlyList<ProductCategory>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Filters and sorts product categories at the database level, optionally restricted to the given ids.
        /// An empty <paramref name="ids"/> list matches all categories.
        /// </summary>
        Task<IReadOnlyList<ProductCategory>> SearchByIdsAsync(
            IList<Guid> ids,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            CancellationToken cancellationToken);

        /// <summary>
        /// Filters, sorts, and pages product categories for a user at the database level, returning only the requested page.
        /// </summary>
        Task<PagedQueryResult<ProductCategory>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}