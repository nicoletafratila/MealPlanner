using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Http
{
    public class ShopService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ShopService> logger)
        : ServiceBase(httpClient, tokenProvider), IShopService
    {
        private readonly string _controller = MealPlannerControllers.ShopUrl;

        public async Task<ShopEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ShopEditModel>(url, cancellationToken);
        }

        public Task<PagedList<ShopModel>?> SearchAsync(QueryParameters<ShopModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<CommandResponse?> AddAsync(ShopEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                return await PostAsync(_controller, model, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Shop AddAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ShopEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                return await PutAsync(_controller, model, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Shop UpdateAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try
            {
                return await DeleteAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Shop DeleteAsync failed. Id {Id}", id);
                throw;
            }
        }
    }
}
