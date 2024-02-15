using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class UnitRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Unit, int>(dbContext), IUnitRepository
    {
    }
}
