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
    public class ShoppingListService(HttpClient httpClient, ITokenProvider tokenProvider, IMemoryCache cache, ILogger<ShoppingListService> logger)
        : ServiceBase(httpClient, tokenProvider), IShoppingListService
    {
        private readonly string _controller = MealPlannerControllers.ShoppingListUrl;
        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public async Task<ShoppingListEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ShoppingListEditModel>(url, cancellationToken);
        }

        public async Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters<ShoppingListModel>? queryParameters = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = SearchCacheKeyBuilder.Build("shoppingLists", queryParameters);
            if (cache.TryGetValue(cacheKey, out PagedList<ShoppingListModel>? cached))
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

        public async Task<ShoppingListEditModel?> MakeShoppingListAsync(ShoppingListCreateModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PostAsync<ShoppingListCreateModel, ShoppingListEditModel>($"{_controller}/{MealPlannerControllers.MakeShoppingListRoute}", model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MakeShoppingListAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(ShoppingListEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PostAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ShoppingList AddAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ShoppingListEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PutAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ShoppingList UpdateAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateProductCollectedAsync(
            Guid shoppingListId,
            Guid productId,
            bool collected,
            CancellationToken cancellationToken = default)
        {
            var model = new ShoppingListProductCollectedModel(shoppingListId, productId, collected);
            try
            {
                var r = await PatchAsync($"{_controller}/{MealPlannerControllers.UpdateProductCollectedRoute}", model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ShoppingList UpdateProductCollectedAsync failed. Model {@Model}", model);
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
                logger.LogError(ex, "ShoppingList DeleteAsync failed. Id {Id}", id);
                throw;
            }
        }
    }
}
