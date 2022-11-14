using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class RecipeCategoryRepository : BaseAsyncRepository<RecipeCategory, int>, IRecipeCategoryRepository
    {
        public RecipeCategoryRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }
    }
}
