using Common.Data.DataContext;
using Common.Data.Entities;

namespace Common.Data.Repository
{
    public class LoggerRepository(MealPlannerLogsDbContext dbContext) : BaseAsyncRepository<Log, int>(dbContext), ILoggerRepository
    {
        //protected readonly MealPlannerLogsDbContext DbContext = dbContext;

        //public virtual async Task<Log?> GetByIdAsync(int id)
        //{
        //    return await DbContext.Set<Log>().FindAsync(id);
        //}

        //public async Task<IReadOnlyList<Log>?> GetAllAsync()
        //{
        //    return await DbContext.Set<Log>().ToListAsync();
        //}

        //public async Task DeleteAsync(Log entity)
        //{
        //    DbContext.Set<Log>().Remove(entity);
        //    await DbContext.SaveChangesAsync();
        //}
    }
}
