using MealPlanner.Shared.Models;

namespace MealPlanner.App.Services
{
    public interface IShoppingListService
    {
        Task<ShoppingListModel> Get(int id);
    }
}
