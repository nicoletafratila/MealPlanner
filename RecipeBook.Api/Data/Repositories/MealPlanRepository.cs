using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.Repositories
{
    public class MealPlanRepository : BaseAsyncRepository<MealPlan, int>, IMealPlanRepository
    {
        public MealPlanRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<MealPlan> GetByIdAsyncIncludeRecipes(int id)
        {
            //return await DbContext.MealPlans.Include(x => x.Recipes).ThenInclude(x => x.MealPlan).FirstOrDefaultAsync(item => item.Id == id);
            return await DbContext.MealPlans.FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
