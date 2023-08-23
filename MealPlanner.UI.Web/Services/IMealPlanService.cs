using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<IList<MealPlanModel>?> GetAllAsync();
        Task<EditMealPlanModel?> GetEditAsync(int id);
        Task<EditMealPlanModel?> AddAsync(EditMealPlanModel model);
        Task UpdateAsync(EditMealPlanModel model);
        Task DeleteAsync(int id);
    }
}
