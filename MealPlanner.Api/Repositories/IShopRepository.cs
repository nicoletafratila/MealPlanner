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
