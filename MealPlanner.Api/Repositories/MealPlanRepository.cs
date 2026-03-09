using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class MealPlanRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<MealPlan, int>(dbContext), IMealPlanRepository
    {
        private MealPlannerDbContext Context =>
            DbContext as MealPlannerDbContext
            ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

        public override async Task<MealPlan?> GetByIdAsync(int id)
        {
            return await Context.MealPlans!
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeCategory)
                .FirstOrDefaultAsync(mp => mp.Id == id);
        }

        public async Task<MealPlan?> GetByIdIncludeRecipesAsync(int id)
        {
            return await Context.MealPlans
                .AsNoTracking()
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeCategory)!
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeIngredients)!
                            .ThenInclude(ri => ri.Unit)
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeIngredients)!
                            .ThenInclude(ri => ri.Product)
                                .ThenInclude(p => p!.ProductCategory)
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeIngredients)!
                            .ThenInclude(ri => ri.Product)
                                .ThenInclude(p => p!.BaseUnit)
                .FirstOrDefaultAsync(mp => mp.Id == id);
        }

        public async Task<IList<MealPlanRecipe>> SearchByRecipeCategoryIdsAsync(IList<int> categoryIds)
        {
            if (categoryIds is null || categoryIds.Count == 0)
            {
                return [];
            }

            return await Context.MealPlanRecipes
                .AsNoTracking()
                .Include(x => x.Recipe)
                    .ThenInclude(r => r!.RecipeCategory)
                .Where(mpr => categoryIds.Contains(mpr.Recipe!.RecipeCategoryId))
                .ToListAsync();
        }

        public async Task<IList<KeyValuePair<Product, MealPlan>>> SearchByProductCategoryIdsAsync(IList<int> categoryIds)
        {
            if (categoryIds is null || categoryIds.Count == 0)
            {
                return [];
            }

            var query =
                from ri in Context.RecipeIngredients.AsNoTracking()
                where ri.Product != null && categoryIds.Contains(ri.Product.ProductCategoryId)
                join mr in Context.MealPlanRecipes.AsNoTracking()
                    on ri.RecipeId equals mr.RecipeId
                select new { ri.Product, mr.MealPlan };

            var results = await query.ToListAsync();

            return results
                .Where(x => x.Product != null && x.MealPlan != null)
                .Select(x => new KeyValuePair<Product, MealPlan>(x.Product!, x.MealPlan!))
                .ToList();
        }

        public async Task<IList<MealPlan>> SearchByRecipeAsync(int recipeId)
        {
            return await Context.MealPlanRecipes
                .AsNoTracking()
                .Where(mpr => mpr.RecipeId == recipeId)
                .Select(mpr => mpr.MealPlan!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<MealPlan?> SearchAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var normalizedName = name.ToUpperInvariant();

            return await Context.MealPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(mp =>
                    mp.Name != null &&
                    mp.Name.ToUpperInvariant() == normalizedName);
        }
    }
}