using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<IEnumerable<MealPlanModel>> GetAll();
        Task<EditMealPlanModel> Get(int id);
    }
}
