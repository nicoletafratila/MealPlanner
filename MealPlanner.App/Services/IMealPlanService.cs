using RecipeBook.Shared.Models;

namespace MealPlanner.App.Services
{
    public interface IMealPlanService
    {
        Task<IEnumerable<MealPlanModel>> GetAll();
        Task<MealPlanModel> Get(int id);
    }
}
