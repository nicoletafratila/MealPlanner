using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace MealPlanner.Services.Http
{
    public class ShopService(HttpClient httpClient, ITokenProvider tokenProvider, IMemoryCache cache, ILogger<ShopService> logger)
        : ServiceBase(httpClient, tokenProvider), IShopService
    {
        private readonly string _controller = MealPlannerControllers.ShopUrl;
        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public async Task<ShopEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ShopEditModel>(url, cancellationToken);
        }

        public async Task<PagedList<ShopModel>?> SearchAsync(QueryParameters<ShopModel>? queryParameters = null, CancellationToken cancellationToken = default)
        {
            var userId = JwtUserIdExtractor.GetUserId(await TokenProvider.GetTokenAsync(cancellationToken));
            var cacheKey = SearchCacheKeyBuilder.Build("shops", queryParameters, userId);
            if (cache.TryGetValue(cacheKey, out PagedList<ShopModel>? cached))
            {
                return cached;
            }

            var result = await SearchAsync(_controller, queryParameters, cancellationToken);

            if (result is not null)
            {
                var opts = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .AddExpirationToken(new CancellationChangeToken(_cacheToken.Token));
                cache.Set(cacheKey, result, opts);
            }

            return result;
        }

        public async Task<CommandResponse?> AddAsync(ShopEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PostAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
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
                var r = await PutAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
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
                var r = await DeleteAsync(url, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Shop DeleteAsync failed. Id {Id}", id);
                throw;
            }
        }
    }
}
