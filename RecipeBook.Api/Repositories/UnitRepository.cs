using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class UnitRepository : BaseAsyncRepository<Unit, int>, IUnitRepository
    {
        public UnitRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }
    }
}
