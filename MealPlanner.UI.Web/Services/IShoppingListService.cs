using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<ShoppingListModel?> GetByIdAsync(int id);
    }
}
