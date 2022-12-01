using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class IngredientCategoryRepository : BaseAsyncRepository<IngredientCategory, int>, IIngredientCategoryRepository
    {
        public IngredientCategoryRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }
    }
}
