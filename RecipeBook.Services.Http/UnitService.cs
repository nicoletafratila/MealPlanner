using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;
using Microsoft.Extensions.Logging;

namespace RecipeBook.Services.Http
{
    public class UnitService(HttpClient httpClient, ITokenProvider tokenProvider, IMemoryCache cache, ILogger<UnitService> logger)
        : ServiceBase(httpClient, tokenProvider), IUnitService
    {
        private readonly string _controller = RecipeBookControllers.UnitUrl;
        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public async Task<UnitEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<UnitEditModel>(url, cancellationToken);
        }

        public async Task<PagedList<UnitModel>?> SearchAsync(QueryParameters<UnitModel>? queryParameters = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"units:{queryParameters?.PageNumber}:{queryParameters?.PageSize}:{string.Join(",", queryParameters?.Sorting?.Select(s => s.PropertyName) ?? [])}";
            if (cache.TryGetValue(cacheKey, out PagedList<UnitModel>? cached)) return cached;

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

        public async Task<CommandResponse?> AddAsync(UnitEditModel model, CancellationToken cancellationToken = default)
        {
            try { var r = await PostAsync(_controller, model, cancellationToken); InvalidateCache(); return r; }
            catch (Exception ex) { logger.LogError(ex, "Unit AddAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> UpdateAsync(UnitEditModel model, CancellationToken cancellationToken = default)
        {
            try { var r = await PutAsync(_controller, model, cancellationToken); InvalidateCache(); return r; }
            catch (Exception ex) { logger.LogError(ex, "Unit UpdateAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try { var r = await DeleteAsync(url, cancellationToken); InvalidateCache(); return r; }
            catch (Exception ex) { logger.LogError(ex, "Unit DeleteAsync failed. Id {Id}", id); throw; }
        }
    }
}
