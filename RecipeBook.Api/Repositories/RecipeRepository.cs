using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Data.Repositories
{
    public class RecipeRepository : BaseAsyncRepository<Recipe, int>, IRecipeRepository
    {
        public RecipeRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Recipe> GetByIdAsyncIncludeIngredients(int id)
        {
            return await (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.RecipeIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
