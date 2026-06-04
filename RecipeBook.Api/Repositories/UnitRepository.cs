using Common.Data.DataContext;
using Common.Data.Repository;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for unit entities.
    /// </summary>
    public class UnitRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Unit, Guid>(dbContext), IUnitRepository
    {
    }
}
