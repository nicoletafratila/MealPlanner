using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public class MealPlanService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        MealPlannerApiConfig mealPlannerApiConfig,
        ILogger<MealPlanService> logger) : IMealPlanService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _mealPlanController =
            mealPlannerApiConfig.Controllers![MealPlannerControllers.MealPlan]
            ?? throw new ArgumentException("MealPlan controller URL is not configured.", nameof(mealPlannerApiConfig));

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        public async Task<MealPlanEditModel?> GetEditAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                $"{_mealPlanController}/edit",
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            try
            {
                return await httpClient.GetFromJsonAsync<MealPlanEditModel?>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize MealPlanEditModel for id {MealPlanId}", id);
                throw;
            }
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(
            int mealPlanId,
            int shopId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                $"{_mealPlanController}/shoppingListProducts",
                new Dictionary<string, string?>
                {
                    ["mealPlanId"] = mealPlanId.ToString(),
                    ["shopId"] = shopId.ToString()
                });

            try
            {
                return await httpClient.GetFromJsonAsync<IList<ShoppingListProductEditModel>?>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to deserialize ShoppingListProductEditModel list for mealPlanId {MealPlanId}, shopId {ShopId}",
                    mealPlanId,
                    shopId);
                throw;
            }
        }

        public async Task<PagedList<MealPlanModel>?> SearchAsync(
            QueryParameters<MealPlanModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<MealPlanModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<MealPlanModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<MealPlanModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<MealPlanModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString($"{_mealPlanController}/search", query);
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<MealPlanModel>?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<MealPlanModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(
            MealPlanEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PostAsJsonAsync(
                _mealPlanController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AddAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to deserialize CommandResponse for AddAsync. Model {@Model}",
                    model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(
            MealPlanEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PutAsJsonAsync(
                _mealPlanController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to deserialize CommandResponse for UpdateAsync. Model {@Model}",
                    model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                _mealPlanController,
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            using var response = await httpClient.DeleteAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "DeleteAsync failed with status code {StatusCode} for id {Id}",
                    response.StatusCode,
                    id);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to deserialize CommandResponse for DeleteAsync. Id {Id}",
                    id);
                throw;
            }
        }
    }
}