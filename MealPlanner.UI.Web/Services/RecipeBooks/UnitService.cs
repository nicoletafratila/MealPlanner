using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public sealed class UnitService(
    HttpClient httpClient,
    TokenProvider tokenProvider,
    RecipeBookApiConfig recipeBookApiConfig,
    ILogger<UnitService> logger) : IUnitService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _unitController = recipeBookApiConfig.Controllers![RecipeBookControllers.Unit] ?? throw new ArgumentException("Unit controller URL is not configured.", nameof(recipeBookApiConfig));

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        public async Task<UnitEditModel?> GetEditAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                $"{_unitController}/edit",
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            try
            {
                return await httpClient.GetFromJsonAsync<UnitEditModel?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize UnitEditModel for id {UnitId}", id);
                throw;
            }
        }

        public async Task<PagedList<UnitModel>?> SearchAsync(QueryParameters<UnitModel>? queryParameters = null)
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

            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString($"{_unitController}/search", query);
            using var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<UnitModel>?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<UnitModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(UnitEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PostAsJsonAsync(_unitController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit AddAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Unit AddAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(UnitEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PutAsJsonAsync(_unitController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Unit UpdateAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                _unitController,
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            using var response = await httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Unit DeleteAsync failed with status code {StatusCode} for id {Id}",
                    response.StatusCode, id);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Unit DeleteAsync. Id {Id}", id);
                throw;
            }
        }
    }
}
