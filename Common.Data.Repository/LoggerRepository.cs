using Common.Data.DataContext;
using Common.Data.Entities;

namespace Common.Data.Repository
{
    /// <summary>
    /// Async repository for persisting and querying <see cref="Log"/> entries.
    /// </summary>
    public class LoggerRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Log, int>(dbContext), ILoggerRepository
    {
    }
}