using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public interface IMealPlanService
    {
        Task<MealPlanEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);

        Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(
            int mealPlanId,
            int shopId,
            CancellationToken cancellationToken = default);

        Task<PagedList<MealPlanModel>?> SearchAsync(
            QueryParameters<MealPlanModel>? queryParameters = null,
            CancellationToken cancellationToken = default);

        Task<CommandResponse?> AddAsync(
            MealPlanEditModel model,
            CancellationToken cancellationToken = default);

        Task<CommandResponse?> UpdateAsync(
            MealPlanEditModel model,
            CancellationToken cancellationToken = default);

        Task<CommandResponse?> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}