using Common.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.Repositories
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
                    .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<MealPlan> GetByIdAsyncIncludeRecipes(int id)
        {
            return await (DbContext as MealPlannerDbContext).MealPlans
                    .Include(x => x.MealPlanRecipes)
                    .ThenInclude(x => x.Recipe)
                    .ThenInclude(x => x.RecipeIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
