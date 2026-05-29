using Common.Constants;
using Common.Http;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Core.Http
{
    public class MealPlanService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<MealPlanService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            MealPlannerControllers.MealPlanUrl
            ?? throw new ArgumentException("MealPlan controller URL is not configured.");

        public async Task<PagedList<MealPlanModel>?> SearchAsync(
            QueryParameters<MealPlanModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SearchAsync(_controller, queryParameters, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} SearchAsync failed", nameof(MealPlanService));
                return null;
            }
        }

        public async Task<MealPlanEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{MealPlannerControllers.EditRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<MealPlanEditModel>(url, cancellationToken);
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int mealPlanId, int shopId, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{MealPlannerControllers.ShoppingListProductsRoute}",
                new Dictionary<string, string?>
                {
                    [ApiQueryParams.MealPlanId] = mealPlanId.ToString(),
                    [ApiQueryParams.ShopId] = shopId.ToString()
                });
            return await GetAsync<IList<ShoppingListProductEditModel>>(url, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AddAsync(MealPlanEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Add failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} AddAsync failed for model {@Model}", nameof(MealPlanService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(MealPlanEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UpdateAsync failed for model {@Model}", nameof(MealPlanService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
                var response = await DeleteAsync(url, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Delete failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} DeleteAsync failed for id {Id}", nameof(MealPlanService), id);
                return (false, ex.Message);
            }
        }
    }
}
