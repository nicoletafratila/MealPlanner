using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public class RecipeCategoryService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        RecipeBookApiConfig recipeBookApiConfig,
        ILogger<RecipeCategoryService> logger) : IRecipeCategoryService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _recipeCategoryController =
            recipeBookApiConfig.Controllers![RecipeBookControllers.RecipeCategory] 
            ?? throw new ArgumentException("RecipeCategory controller URL is not configured.", nameof(recipeBookApiConfig));

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        public async Task<RecipeCategoryEditModel?> GetEditAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                $"{_recipeCategoryController}/edit",
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            try
            {
                return await httpClient.GetFromJsonAsync<RecipeCategoryEditModel?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize RecipeCategoryEditModel for id {Id}", id);
                throw;
            }
        }

        public async Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters<RecipeCategoryModel>? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<RecipeCategoryModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<RecipeCategoryModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<RecipeCategoryModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<RecipeCategoryModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString($"{_recipeCategoryController}/search", query);
            using var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("RecipeCategory SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<RecipeCategoryModel>?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<RecipeCategoryModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(RecipeCategoryEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PostAsJsonAsync(_recipeCategoryController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("RecipeCategory AddAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for RecipeCategory AddAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(RecipeCategoryEditModel model)
        {
            await EnsureAuthAsync();

            using var response = await httpClient.PutAsJsonAsync(_recipeCategoryController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("RecipeCategory UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for RecipeCategory UpdateAsync. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(IList<RecipeCategoryModel> models)
        {
            await EnsureAuthAsync();

            var url = $"{_recipeCategoryController}/updateAll";
            using var response = await httpClient.PutAsJsonAsync(url, models, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("RecipeCategory bulk UpdateAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for RecipeCategory bulk UpdateAsync. Models {@Models}", models);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                _recipeCategoryController,
                new Dictionary<string, string?>
                {
                    ["id"] = id.ToString()
                });

            using var response = await httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("RecipeCategory DeleteAsync failed with status code {StatusCode} for id {Id}", response.StatusCode, id);
                response.EnsureSuccessStatusCode();
            }

            var stream = await response.Content.ReadAsStreamAsync();

            try
            {
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for RecipeCategory DeleteAsync. Id {Id}", id);
                throw;
            }
        }
    }
}