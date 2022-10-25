using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Data.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan> GetByIdAsyncIncludeRecipes(int id);
    }
}
