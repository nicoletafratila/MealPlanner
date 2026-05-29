using Common.Constants;
using Common.Http;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Core.Http
{
    public class ShopService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ShopService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            MealPlannerControllers.ShopUrl
            ?? throw new ArgumentException("Shop controller URL is not configured.");

        public async Task<PagedList<ShopModel>?> SearchAsync(
            QueryParameters<ShopModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SearchAsync(_controller, queryParameters, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} SearchAsync failed", nameof(ShopService));
                return null;
            }
        }

        public async Task<ShopEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{MealPlannerControllers.EditRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ShopEditModel>(url, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AddAsync(ShopEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Add failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} AddAsync failed for model {@Model}", nameof(ShopService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(ShopEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UpdateAsync failed for model {@Model}", nameof(ShopService), model);
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
                logger.LogError(ex, "{ServiceName} DeleteAsync failed for id {Id}", nameof(ShopService), id);
                return (false, ex.Message);
            }
        }
    }
}
