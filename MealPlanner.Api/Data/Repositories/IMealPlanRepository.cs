using Common.Data.Entities;
using Common.Repository.Repositories;

namespace MealPlanner.Api.Data.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan> GetByIdAsyncIncludeRecipes(int id);
    }
}
