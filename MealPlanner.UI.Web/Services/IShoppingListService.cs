using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<EditShoppingListModel?> GetEditAsync(int id);
        Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<EditShoppingListModel?> SaveShoppingListFromMealPlanAsync(int mealPlanId);
        Task<string?> AddAsync(EditShoppingListModel model);
        Task<string?> UpdateAsync(EditShoppingListModel model);
        Task<string?> DeleteAsync(int id);
    }
}