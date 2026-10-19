using Common.Data.Repository;
using Common.Pagination;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="Job"/> entities.
    /// </summary>
    public interface IJobRepository : IAsyncRepository<Job, Guid>
    {
        /// <summary>
        /// Gets all jobs still waiting to be picked up by the dispatcher.
        /// </summary>
        Task<IReadOnlyList<Job>> GetPendingAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets jobs left in the Running state, which can only happen if a prior process crashed mid-job.
        /// </summary>
        Task<IReadOnlyList<Job>> GetOrphanedRunningAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Filters, sorts, and pages share jobs across all users, for the admin history view.
        /// </summary>
        Task<PagedQueryResult<Job>> SearchAsync(
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
