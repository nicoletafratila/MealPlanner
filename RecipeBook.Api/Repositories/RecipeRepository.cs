using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    public class RecipeRepository : BaseAsyncRepository<Recipe, int>, IRecipeRepository
    {
        public RecipeRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<IReadOnlyList<Recipe>> GetAllAsync()
        {
            return (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.Category).ToList();
        }

        public override async Task<Recipe> GetByIdAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<Recipe> GetByIdAsyncIncludeIngredients(int id)
        {
            return await (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.Category)
                    .Include(x => x.RecipeIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
