using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<IList<ShoppingListModel>?> GetAllAsync();
        Task<EditShoppingListModel?> GetEditAsync(int id);
        Task<EditShoppingListModel?> GetShoppingListFromMealPlanAsync(int mealPlanId);
        Task UpdateAsync(EditShoppingListModel model);
        Task DeleteAsync(int id);
    }
}