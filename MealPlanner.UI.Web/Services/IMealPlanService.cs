using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<MealPlanEditModel?> GetEditAsync(int id);
        Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int mealPlanId, int shopId);
        Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<CommandResponse?> AddAsync(MealPlanEditModel model);
        Task<CommandResponse?> UpdateAsync(MealPlanEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}
