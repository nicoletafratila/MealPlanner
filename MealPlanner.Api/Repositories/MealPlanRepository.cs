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
            return await (DbContext as MealPlannerDbContext)!.MealPlans
                    .Include(x => x.MealPlanRecipes)!
                        .ThenInclude(x => x.Recipe)
                            .ThenInclude(x => x!.RecipeCategory)
                    .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MealPlan?> GetByIdIncludeRecipesAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeCategory)
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeIngredients)!
                          .ThenInclude(x => x.Unit)
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeIngredients)!
                          .ThenInclude(x => x.Product)
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeIngredients)!
                          .ThenInclude(x => x.Product)
                             .ThenInclude(x => x!.ProductCategory)
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeIngredients)!
                          .ThenInclude(x => x.Product)
                             .ThenInclude(x => x!.BaseUnit)
              .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IList<MealPlanRecipe>> SearchByRecipeCategoryIdsAsync(IList<int> categoryIds)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlanRecipes
                  .Include(x => x.Recipe)
                  .ThenInclude(x => x!.RecipeCategory)
                  .Where(item => categoryIds.Contains(item.Recipe!.RecipeCategoryId)).ToListAsync();
        }

        public async Task<IList<KeyValuePair<Product, MealPlan>>?> SearchByProductCategoryIdsAsync(IList<int> categoryIds)
        {
            var productPerRecipe = await (DbContext as MealPlannerDbContext)!.RecipeIngredients
                                         .Where(x => categoryIds.Contains(x.Product!.ProductCategoryId))
                                         .Select(x => new { x.Product, x.Recipe })
                                         .ToListAsync();
            var recipesWhereUsed = productPerRecipe.Select(i => i.Recipe?.Id);
            var mealPlansPerRecipe = await (DbContext as MealPlannerDbContext)!.MealPlanRecipes
                    .Select(x => new { x.MealPlan, x.Recipe })
                    .Where(x => recipesWhereUsed.Contains(x.Recipe!.Id)).ToListAsync();

            return productPerRecipe.Join(
                    mealPlansPerRecipe,
                    product => product.Recipe?.Id,
                    mealPlan => mealPlan.Recipe?.Id, (product, mealPlan) => new KeyValuePair<Product, MealPlan>(product.Product!, mealPlan.MealPlan!)).ToList();
        }

        public async Task<IList<MealPlan>?> SearchByRecipeAsync(int recipeId)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
                    .Where(item => item.MealPlanRecipes!.Any(r => r.RecipeId == recipeId))
                    .ToListAsync();
        }

        public async Task<MealPlan?> SearchAsync(string name)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
                   .FirstOrDefaultAsync(item => item.Name!.ToLower() == name.ToLower());
        }
    }
}
