using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public class RecipeService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        RecipeBookApiConfig recipeBookApiConfig,
        ILogger<RecipeService> logger) : IRecipeService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _recipeController =
            recipeBookApiConfig.Controllers![RecipeBookControllers.Recipe]
            ?? throw new ArgumentException("Recipe controller URL is not configured.", nameof(recipeBookApiConfig));

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        public async Task<RecipeModel?> GetByIdAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                _recipeController,
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            try
            {
                return await httpClient.GetFromJsonAsync<RecipeModel?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize RecipeModel for id {RecipeId}", id);
                throw;
            }
        }

        public async Task<RecipeEditModel?> GetEditAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                $"{_recipeController}/edit",
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            try
            {
                return await httpClient.GetFromJsonAsync<RecipeEditModel?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize RecipeEditModel for id {RecipeId}", id);
                throw;
            }
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int recipeId, int shopId)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                $"{_recipeController}/shoppingListProducts",
                new Dictionary<string, string?>
                {
                    ["recipeId"] = recipeId.ToString(),
                    ["shopId"] = shopId.ToString()
                });

            try
            {
                return await httpClient.GetFromJsonAsync<IList<ShoppingListProductEditModel>?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex,
                    "Failed to deserialize ShoppingListProductEditModel list for recipeId {RecipeId}, shopId {ShopId}",
                    recipeId, shopId);
                throw;
            }
        }

        public async Task<PagedList<RecipeModel>?> SearchAsync(QueryParameters<RecipeModel>? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<RecipeModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<RecipeModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<RecipeModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<RecipeModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString($"{_recipeController}/search", query);
            using var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Recipe SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<RecipeModel>?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<RecipeModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(RecipeEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PostAsJsonAsync(_recipeController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Recipe AddAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Recipe AddAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(RecipeEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PutAsJsonAsync(_recipeController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Recipe UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for Recipe UpdateAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                _recipeController,
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            using var response = await httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Recipe DeleteAsync failed with status code {StatusCode} for id {Id}",
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
                logger.LogError(ex, "Failed to deserialize CommandResponse for Recipe DeleteAsync. Id {Id}", id);
                throw;
            }
        }
    }
}