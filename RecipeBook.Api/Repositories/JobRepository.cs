using Common.Data.DataContext;
using Common.Data.Repository;
using Common.Pagination;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Job"/> entities.
    /// </summary>
    public class JobRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Job, Guid>(dbContext), IJobRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public async Task<IReadOnlyList<Job>> GetPendingAsync(CancellationToken cancellationToken)
        {
            return await Context.Jobs
                .Where(j => j.Status == JobStatus.Pending)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Job>> GetOrphanedRunningAsync(CancellationToken cancellationToken)
        {
            return await Context.Jobs
                .Where(j => j.Status == JobStatus.Running)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedQueryResult<Job>> SearchAsync(
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IQueryable<Job> query = Context.Jobs.OrderBy(x => x.Id);

            query = query.ApplyFilters(filters).ApplySorting(sorting);

            return await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
