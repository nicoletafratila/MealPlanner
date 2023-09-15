using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    public class RecipeIngredientRepository : IRecipeIngredientRepository
    {
        protected readonly MealPlannerDbContext DbContext;

        public RecipeIngredientRepository(MealPlannerDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<IReadOnlyList<RecipeIngredient>> SearchAsync(int productId)
        {
            return await (DbContext as MealPlannerDbContext)!.RecipeIngredients.Where(x => x.ProductId == productId).ToListAsync();
        }
    }
}
