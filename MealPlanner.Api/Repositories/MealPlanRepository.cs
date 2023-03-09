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

        public override async Task<MealPlan> GetByIdAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext).MealPlans
                    .Include(x => x.MealPlanRecipes)
                        .ThenInclude(x => x.Recipe)
                            .ThenInclude(x => x.RecipeCategory)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<MealPlan> GetByIdAsyncIncludeRecipes(int id)
        {
            return await (DbContext as MealPlannerDbContext).MealPlans
                    .Include(x => x.MealPlanRecipes)
                        .ThenInclude(x => x.Recipe)
                            .ThenInclude(x => x.RecipeCategory)
                    .Include(x => x.MealPlanRecipes)
                        .ThenInclude(x => x.Recipe)
                            .ThenInclude(x => x.RecipeIngredients)
                                .ThenInclude(x => x.Ingredient)
                                    .ThenInclude(x => x.Unit)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
