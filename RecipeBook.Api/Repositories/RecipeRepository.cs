using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Recipe"/> entities with eager-loading helpers.
    /// </summary>
    public class RecipeRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Recipe, int>(dbContext), IRecipeRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public override async Task<IReadOnlyList<Recipe>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .ToListAsync(cancellationToken);
        }

        public override async Task<Recipe?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Recipe?> GetByIdIncludeIngredientsAsync(
            int? id,
            CancellationToken cancellationToken = default)
        {
            if (id is null)
                return null;

            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)!.ThenInclude(p => p!.ProductCategory)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)!.ThenInclude(p => p!.BaseUnit)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Product)
                .Include(x => x.RecipeIngredients)!.ThenInclude(ri => ri.Unit)
                .FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
        }

        public async Task<IReadOnlyList<Recipe>> SearchAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .Where(x => x.RecipeCategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Recipe?> SearchAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await Context.Recipes
                .Include(x => x.RecipeCategory)
                .FirstOrDefaultAsync(
                    x => x.Name != null && x.Name.ToLower() == name.ToLower(),
                    cancellationToken);
        }
    }
}