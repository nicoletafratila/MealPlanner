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
                    .FirstOrDefaultAsync(item => item.Id == id);
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
                          .ThenInclude(x => x.Product)
                             .ThenInclude(x => x!.ProductCategory)
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeIngredients)!
                          .ThenInclude(x => x.Product)
                            .ThenInclude(x => x!.Unit)
              .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IList<MealPlan>?> SearchByRecipeCategoryId(int categoryId)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
              .Include(x => x.MealPlanRecipes)!
                  .ThenInclude(x => x.Recipe)
                      .ThenInclude(x => x!.RecipeCategory)
              .Where(item => item.MealPlanRecipes!.Any(r => r.Recipe!.RecipeCategoryId == categoryId))
              .ToListAsync();
        }

        public async Task<IList<MealPlan>?> SearchByProductCategoryId(int categoryId)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
             .Include(x => x.MealPlanRecipes)!
                 .ThenInclude(x => x.Recipe)
                     .ThenInclude(x => x!.RecipeCategory)
            .Include(x => x.MealPlanRecipes)!
                 .ThenInclude(x => x.Recipe)
                     .ThenInclude(x => x!.RecipeIngredients)!
                         .ThenInclude(x => x.Product)
                            .ThenInclude(x => x!.ProductCategory)
             .Where(item => item.MealPlanRecipes!.Any(r => r.Recipe!.RecipeIngredients!.Any(i => i.Product!.ProductCategoryId == categoryId)))
             .ToListAsync();
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
                   .FirstOrDefaultAsync(item => item.Name!.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
