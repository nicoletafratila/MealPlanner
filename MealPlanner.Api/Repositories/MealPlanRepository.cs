using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class MealPlanRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<MealPlan, int>(dbContext), IMealPlanRepository
    {
        public override async Task<MealPlan?> GetByIdAsync(int id)
        {
            var context = (MealPlannerDbContext)DbContext;

            return await context.MealPlans!
                .Include(mp => mp.MealPlanRecipes)!
                    .ThenInclude(mpr => mpr.Recipe)
                        .ThenInclude(r => r!.RecipeCategory)
                .FirstOrDefaultAsync(mp => mp.Id == id);
        }

        public async Task<MealPlan?> GetByIdIncludeRecipesAsync(int id)
        {
            var context = (MealPlannerDbContext)DbContext;

            return await context.MealPlans
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
            if (categoryIds == null || categoryIds.Count == 0)
            {
                return new List<MealPlanRecipe>();
            }

            var context = (MealPlannerDbContext)DbContext;

            return await context.MealPlanRecipes
                .AsNoTracking()
                .Include(x => x.Recipe)
                    .ThenInclude(r => r!.RecipeCategory)
                .Where(mpr => categoryIds.Contains(mpr.Recipe!.RecipeCategoryId))
                .ToListAsync();
        }

        public async Task<IList<KeyValuePair<Product, MealPlan>>> SearchByProductCategoryIdsAsync(IList<int> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count == 0)
            {
                return new List<KeyValuePair<Product, MealPlan>>();
            }

            var context = (MealPlannerDbContext)DbContext;

            var query =
                from ri in context.RecipeIngredients.AsNoTracking()
                where categoryIds.Contains(ri.Product!.ProductCategoryId)
                join mr in context.MealPlanRecipes.AsNoTracking()
                    on ri.RecipeId equals mr.RecipeId
                select new { ri.Product, mr.MealPlan };

            var results = await query.ToListAsync();

            return results
                .Select(x => new KeyValuePair<Product, MealPlan>(x.Product!, x.MealPlan!))
                .ToList();
        }

        public async Task<IList<MealPlan>> SearchByRecipeAsync(int recipeId)
        {
            var context = (MealPlannerDbContext)DbContext;

            return await context.MealPlanRecipes
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

            var context = (MealPlannerDbContext)DbContext;
            var normalizedName = name.ToUpper();

            return await context.MealPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(mp => mp.Name!.ToUpper() == normalizedName);
        }
    }
}
