using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Recipe"/> entities with eager-loading helpers.
    /// </summary>
    public class RecipeRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Recipe, int>(dbContext), IRecipeRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public override async Task<IReadOnlyList<Recipe>> GetAllAsync()
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .ToListAsync();
        }

        public override async Task<Recipe?> GetByIdAsync(int id)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Recipe?> GetByIdIncludeIngredientsAsync(int? id)
        {
            if (id is null)
                return null;

            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)!.ThenInclude(p => p!.ProductCategory)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)!.ThenInclude(p => p!.BaseUnit)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Unit)
                .FirstOrDefaultAsync(x => x.Id == id.Value);
        }

        public async Task<IReadOnlyList<Recipe>> SearchAsync(int categoryId)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Where(x => x.RecipeCategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Recipe?> SearchAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .FirstOrDefaultAsync(x =>
                    x.Name != null &&
                    x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}