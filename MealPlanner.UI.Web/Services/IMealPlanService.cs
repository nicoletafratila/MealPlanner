using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<IList<MealPlanModel>> GetAll();
        Task<EditMealPlanModel> Get(int id);
        Task<EditMealPlanModel> Add(EditMealPlanModel model);
        Task Update(EditMealPlanModel model);
        Task DeleteAsync(int id);
    }
}
