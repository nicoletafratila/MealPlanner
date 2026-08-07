using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public class RecipeService(HttpClient httpClient, ITokenProvider tokenProvider, IMemoryCache cache, ILogger<RecipeService> logger)
        : ServiceBase(httpClient, tokenProvider), IRecipeService
    {
        private readonly string _controller = RecipeBookControllers.RecipeUrl;
        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public async Task<RecipeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try
            {
                return await GetAsync<RecipeModel>(url, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch RecipeModel for id {RecipeId}", id); return null;
            }
        }

        public async Task<RecipeEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<RecipeEditModel>(url, cancellationToken);
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(Guid recipeId, Guid shopId, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.ShoppingListProductsRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.RecipeId] = recipeId.ToString(), [ApiQueryParams.ShopId] = shopId.ToString() });
            return await GetAsync<IList<ShoppingListProductEditModel>>(url, cancellationToken);
        }

        public async Task<PagedList<RecipeModel>?> SearchAsync(QueryParameters<RecipeModel>? queryParameters = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = SearchCacheKeyBuilder.Build("recipes", queryParameters);
            if (cache.TryGetValue(cacheKey, out PagedList<RecipeModel>? cached))
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

        public async Task<CommandResponse?> AddAsync(RecipeEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PostAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Recipe AddAsync failed. Model {@Model}", model); throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(RecipeEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PutAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Recipe UpdateAsync failed. Model {@Model}", model); throw;
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
                logger.LogError(ex, "Recipe DeleteAsync failed. Id {Id}", id); throw;
            }
        }
    }
}
