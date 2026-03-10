using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public class ProductCategoryService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        RecipeBookApiConfig recipeBookApiConfig,
        ILogger<ProductCategoryService> logger) : IProductCategoryService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _productCategoryController =
            recipeBookApiConfig.Controllers![RecipeBookControllers.ProductCategory]
            ?? throw new ArgumentException("ProductCategory controller URL is not configured.", nameof(recipeBookApiConfig));

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        public async Task<ProductCategoryEditModel?> GetEditAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                $"{_productCategoryController}/edit",
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            try
            {
                return await httpClient.GetFromJsonAsync<ProductCategoryEditModel?>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize ProductCategoryEditModel for id {Id}", id);
                throw;
            }
        }

        public async Task<PagedList<ProductCategoryModel>?> SearchAsync(
            QueryParameters<ProductCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<ProductCategoryModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<ProductCategoryModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<ProductCategoryModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<ProductCategoryModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString($"{_productCategoryController}/search", query);
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ProductCategory SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<ProductCategoryModel>?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<ProductCategoryModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(
            ProductCategoryEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PostAsJsonAsync(
                _productCategoryController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ProductCategory AddAsync failed with status code {StatusCode}", response.StatusCode);
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
                    "Failed to deserialize CommandResponse for ProductCategory AddAsync. Model {@Model}",
                    model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(
            ProductCategoryEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PutAsJsonAsync(
                _productCategoryController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ProductCategory UpdateAsync failed with status code {StatusCode}", response.StatusCode);
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
                    "Failed to deserialize CommandResponse for ProductCategory UpdateAsync. Model {@Model}",
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
                _productCategoryController,
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            using var response = await httpClient.DeleteAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "ProductCategory DeleteAsync failed with status code {StatusCode} for id {Id}",
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
                    "Failed to deserialize CommandResponse for ProductCategory DeleteAsync. Id {Id}",
                    id);
                throw;
            }
        }
    }
}