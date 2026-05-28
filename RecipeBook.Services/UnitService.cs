using System.Text.Json;
using Common.Api;
using RecipeBook.Shared.Constants;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RecipeBook.Api;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services
{
    public class UnitService(
        HttpClient httpClient,
        ITokenProvider tokenProvider,
        RecipeBookApiConfig recipeBookApiConfig,
        IMemoryCache cache,
        ILogger<UnitService> logger) : IUnitService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _unitController =
            recipeBookApiConfig.Controllers![RecipeBookControllers.Unit]
            ?? throw new ArgumentException("Unit controller URL is not configured.", nameof(recipeBookApiConfig));

        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        public async Task<UnitEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                $"{_unitController}/edit",
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            try
            {
                return await httpClient.GetFromJsonAsync<UnitEditModel?>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize UnitEditModel for id {UnitId}", id);
                throw;
            }
        }

        public async Task<PagedList<UnitModel>?> SearchAsync(
            QueryParameters<UnitModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<UnitModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<UnitModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<UnitModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<UnitModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            var cacheKey = $"units:{JsonSerializer.Serialize(query, JsonOptions)}";

            if (cache.TryGetValue(cacheKey, out PagedList<UnitModel>? cached))
                return cached;

            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString($"{_unitController}/search", query);

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            PagedList<UnitModel>? result;
            try
            {
                result = await JsonSerializer.DeserializeAsync<PagedList<UnitModel>?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<UnitModel> for query {@Query}", query);
                throw;
            }

            var entryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .AddExpirationToken(new CancellationChangeToken(_cacheToken.Token));

            cache.Set(cacheKey, result, entryOptions);
            return result;
        }

        public async Task<CommandResponse?> AddAsync(
            UnitEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PostAsJsonAsync(
                _unitController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit AddAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                var result = await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);

                InvalidateCache();
                return result;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Unit AddAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(
            UnitEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PutAsJsonAsync(
                _unitController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                var result = await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);

                InvalidateCache();
                return result;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Unit UpdateAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                _unitController,
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            using var response = await httpClient.DeleteAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit DeleteAsync failed with status code {StatusCode} for id {Id}",
                    response.StatusCode, id);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                var result = await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);

                InvalidateCache();
                return result;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Unit DeleteAsync. Id {Id}", id);
                throw;
            }
        }
    }
}
