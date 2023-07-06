using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<ShoppingListModel?> GetByIdAsync(int id);
        Task<EditShoppingListModel?> AddAsync(EditShoppingListModel model);
        Task UpdateAsync(EditShoppingListModel model);
    }
}
