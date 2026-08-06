using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public class ProductCategoryService(HttpClient httpClient, ITokenProvider tokenProvider, IMemoryCache cache, ILogger<ProductCategoryService> logger)
        : ServiceBase(httpClient, tokenProvider), IProductCategoryService
    {
        private readonly string _controller = RecipeBookControllers.ProductCategoryUrl;
        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public async Task<ProductCategoryEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ProductCategoryEditModel>(url, cancellationToken);
        }

        public async Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters<ProductCategoryModel>? queryParameters = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = SearchCacheKeyBuilder.Build("productCategories", queryParameters);
            if (cache.TryGetValue(cacheKey, out PagedList<ProductCategoryModel>? cached))
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

        public async Task<CommandResponse?> AddAsync(ProductCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PostAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProductCategory AddAsync failed. Model {@Model}", model); throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ProductCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PutAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProductCategory UpdateAsync failed. Model {@Model}", model); throw;
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
                logger.LogError(ex, "ProductCategory DeleteAsync failed. Id {Id}", id); throw;
            }
        }
    }
}
