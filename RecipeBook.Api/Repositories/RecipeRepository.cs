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
                    .Include(x => x.RecipeCategory).ToList();
        }

        public override async Task<Recipe> GetByIdAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.RecipeCategory)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IReadOnlyList<Recipe>> SearchAsync(int categoryId)
        {
            return (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.RecipeCategory)
                    .Where(x => x.RecipeCategoryId == categoryId).ToList();
        }

        public async Task<Recipe> GetByIdIncludeIngredientsAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext).Recipes
                    .Include(x => x.RecipeCategory)
                    .Include(x => x.RecipeIngredients)
                        .ThenInclude(x => x.Ingredient)
                            .ThenInclude(x => x.IngredientCategory)
                    .Include(x => x.RecipeIngredients)
                        .ThenInclude(x => x.Ingredient)
                            .ThenInclude(x => x.Unit)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
