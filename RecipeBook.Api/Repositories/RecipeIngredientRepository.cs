using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    public class RecipeIngredientRepository(MealPlannerDbContext dbContext) : IRecipeIngredientRepository
    {
        protected readonly MealPlannerDbContext DbContext = dbContext;

        public async Task<IReadOnlyList<RecipeIngredient>?> GetAllAsync()
        {
            return await DbContext!.RecipeIngredients
                        .Include(x => x.Unit).ToListAsync();
        }

        public async Task<IReadOnlyList<RecipeIngredient>?> SearchAsync(int productId)
        {
            return await DbContext!.RecipeIngredients.Where(x => x.ProductId == productId).ToListAsync();
        }
    }
}
