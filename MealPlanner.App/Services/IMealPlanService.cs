using RecipeBook.Shared.Models;

namespace MealPlanner.App.Services
{
    public interface IMealPlanService
    {
        Task<IEnumerable<MealPlanModel>> GetAll();
        Task<EditMealPlanModel> Get(int id);
    }
}
