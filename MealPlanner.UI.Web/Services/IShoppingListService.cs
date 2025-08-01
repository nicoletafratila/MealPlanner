using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IShoppingListService
    {
        Task<ShoppingListEditModel?> GetEditAsync(int id);
        Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<ShoppingListEditModel?> MakeShoppingListAsync(ShoppingListCreateModel model);
        Task<CommandResponse?> AddAsync(ShoppingListEditModel model);
        Task<CommandResponse?> UpdateAsync(ShoppingListEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}