using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<ShoppingListEditModel?> GetEditAsync(int id);
        Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<ShoppingListEditModel?> MakeShoppingListAsync(ShoppingListCreateModel model);
        Task<string?> AddAsync(ShoppingListEditModel model);
        Task<string?> UpdateAsync(ShoppingListEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}