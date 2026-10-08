using Common.Data.Repository;
using Common.Pagination;
using MealPlanner.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public interface IShopRepository : IAsyncRepository<Shop, Guid>
    {
        Task<IReadOnlyList<Shop>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<Shop?> GetByIdIncludeDisplaySequenceAsync(Guid? id, CancellationToken cancellationToken);

        /// <summary>
        /// Loads every shop owned by the given user with its display sequence (and referenced product categories)
        /// included, optionally filtered the same way the shops grid is.
        /// </summary>
        Task<IReadOnlyList<Shop>> GetAllByUserIncludeDisplaySequenceAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            CancellationToken cancellationToken);

        /// <summary>
        /// Filters, sorts, and pages shops for a user at the database level, returning only the requested page.
        /// </summary>
        Task<PagedQueryResult<Shop>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
