using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public sealed class ShopService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        MealPlannerApiConfig mealPlannerApiConfig,
        ILogger<ShopService> logger) : IShopService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _shopController =
            mealPlannerApiConfig.Controllers![MealPlannerControllers.Shop]
            ?? throw new ArgumentException("Shop controller URL is not configured.", nameof(mealPlannerApiConfig));

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        public async Task<ShopEditModel?> GetEditAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                $"{_shopController}/edit",
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            try
            {
                return await httpClient.GetFromJsonAsync<ShopEditModel?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize ShopEditModel for id {Id}", id);
                throw;
            }
        }

        public async Task<PagedList<ShopModel>?> SearchAsync(QueryParameters<ShopModel>? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<ShopModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<ShopModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<ShopModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<ShopModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString($"{_shopController}/search", query);
            using var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Shop SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<ShopModel>?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<ShopModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(ShopEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PostAsJsonAsync(_shopController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Shop AddAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Shop AddAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ShopEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PutAsJsonAsync(_shopController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Shop UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Shop UpdateAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                _shopController,
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            using var response = await httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Shop DeleteAsync failed with status code {StatusCode} for id {Id}",
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
                logger.LogError(ex, "Failed to deserialize CommandResponse for Shop DeleteAsync. Id {Id}", id);
                throw;
            }
        }
    }
}