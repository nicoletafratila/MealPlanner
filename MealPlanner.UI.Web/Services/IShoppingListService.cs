using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<IList<ShoppingListModel>?> GetAllAsync();
        Task<ShoppingListModel?> GetByIdAsync(int id);
        Task<EditShoppingListModel?> GetEditAsync(int id);
        Task<EditShoppingListModel?> GetShoppingListFromMealPlanAsync(int mealPlanId);
        Task<EditShoppingListModel?> AddAsync(EditShoppingListModel model);
        Task UpdateAsync(EditShoppingListModel model);
        Task DeleteAsync(int id);
    }
}
