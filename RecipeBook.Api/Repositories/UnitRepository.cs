using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for unit entities.
    /// </summary>
    public class UnitRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Unit, int>(dbContext), IUnitRepository
    {
    }
}
