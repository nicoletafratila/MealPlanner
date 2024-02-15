using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class RecipeCategoryRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<RecipeCategory, int>(dbContext), IRecipeCategoryRepository
    {
    }
}
