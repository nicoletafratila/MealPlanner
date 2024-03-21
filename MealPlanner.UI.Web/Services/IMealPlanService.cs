using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IMealPlanService
    {
        Task<EditMealPlanModel?> GetEditAsync(int id);
        Task<IList<ShoppingListProductModel>?> GetShoppingListProducts(int mealPlanId, int shopId);
        Task<IList<MealPlanModel>?> GetAllAsync();
        Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(EditMealPlanModel model);
        Task<string?> UpdateAsync(EditMealPlanModel model);
        Task<string?> DeleteAsync(int id);
    }
}
