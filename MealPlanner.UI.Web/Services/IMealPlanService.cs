using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<MealPlanEditModel?> GetEditAsync(int id);
        Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int mealPlanId, int shopId);
        Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(MealPlanEditModel model);
        Task<string?> UpdateAsync(MealPlanEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}
