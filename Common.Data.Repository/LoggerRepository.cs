using Common.Data.DataContext;
using Common.Data.Entities;

namespace Common.Data.Repository
{
    public class LoggerRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Log, int>(dbContext), ILoggerRepository
    {
    }
}
