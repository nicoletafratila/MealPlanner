using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class MealPlanRepository : BaseAsyncRepository<MealPlan, int>, IMealPlanRepository
    {
        public MealPlanRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

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

        public async Task<IList<MealPlan>> SearchByRecipeAsync(int recipeId)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
                    .Where(item => item.MealPlanRecipes!.Any(r => r.RecipeId == recipeId)).ToListAsync();
        }

        public async Task<MealPlan?> SearchAsync(string name)
        {
            return await (DbContext as MealPlannerDbContext)!.MealPlans
                   .FirstOrDefaultAsync(item => item.Name!.ToLower() == name.ToLower());
        }
    }
}
