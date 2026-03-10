using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public class ProductService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        RecipeBookApiConfig recipeBookApiConfig,
        ILogger<ProductService> logger) : IProductService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _productController =
            recipeBookApiConfig.Controllers![RecipeBookControllers.Product]
            ?? throw new ArgumentException("Product controller URL is not configured.", nameof(recipeBookApiConfig));

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        public async Task<ProductEditModel?> GetEditAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                $"{_productController}/edit",
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            try
            {
                return await httpClient.GetFromJsonAsync<ProductEditModel?>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize ProductEditModel for id {Id}", id);
                throw;
            }
        }

        public async Task<PagedList<ProductModel>?> SearchAsync(
            QueryParameters<ProductModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<ProductModel>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<ProductModel>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<ProductModel>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<ProductModel>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString($"{_productController}/search", query);
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Product SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<ProductModel>?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<ProductModel> for query {@Query}", query);
                throw;
            }
        }

        public async Task<CommandResponse?> AddAsync(
            ProductEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PostAsJsonAsync(
                _productController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Product AddAsync failed with status code {StatusCode}", response.StatusCode);
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
                    "Failed to deserialize CommandResponse for Product AddAsync. Model {@Model}",
                    model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(
            ProductEditModel model,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PutAsJsonAsync(
                _productController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Product UpdateAsync failed with status code {StatusCode}", response.StatusCode);
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
                    "Failed to deserialize CommandResponse for Product UpdateAsync. Model {@Model}",
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
                _productController,
                new Dictionary<string, string?> { ["id"] = id.ToString() });

            using var response = await httpClient.DeleteAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Product DeleteAsync failed with status code {StatusCode} for id {Id}",
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
                    "Failed to deserialize CommandResponse for Product DeleteAsync. Id {Id}",
                    id);
                throw;
            }
        }
    }
}