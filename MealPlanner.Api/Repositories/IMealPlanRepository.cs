using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan?> GetByIdIncludeRecipesAsync(int id);
    }
}
