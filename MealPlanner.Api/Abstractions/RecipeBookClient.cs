using System.Text.Json;
using Common.Http;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Abstractions
{
    public class RecipeBookClient(HttpClient httpClient, RecipeBookClientConfig config) : IRecipeBookClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly RecipeBookClientConfig _config = config ?? throw new ArgumentNullException(nameof(config));

        public async Task<IList<RecipeCategoryModel>?> GetRecipeCategoriesAsync(
            string categoryIds,
            string? authToken,
            IEnumerable<FilterItem>? filters = null,
            IEnumerable<SortingModel>? sorting = null,
            CancellationToken cancellationToken = default)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);

            var query = BuildSearchByCategoriesQuery(categoryIds, filters, sorting);

            var controller = _config.Controllers![RecipeCategoryRoutes.ControllerName];
            var url = QueryHelpers.AddQueryString($"{controller}/searchbycategories", query);

            return await _httpClient.GetFromJsonAsync<IList<RecipeCategoryModel>>(url, cancellationToken);
        }

        public async Task<IList<ProductCategoryModel>?> GetProductCategoriesAsync(
            string categoryIds,
            string? authToken,
            IEnumerable<FilterItem>? filters = null,
            IEnumerable<SortingModel>? sorting = null,
            CancellationToken cancellationToken = default)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);

            var query = BuildSearchByCategoriesQuery(categoryIds, filters, sorting);

            var controller = _config.Controllers![ProductCategoryRoutes.ControllerName];
            var url = QueryHelpers.AddQueryString($"{controller}/searchbycategories", query);

            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>(url, cancellationToken);
        }

        private static Dictionary<string, string?> BuildSearchByCategoriesQuery(
            string categoryIds,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting) => new()
        {
            ["categoryIds"] = categoryIds,
            ["filters"] = filters is null ? null : JsonSerializer.Serialize(filters, JsonOptions),
            ["sorting"] = sorting is null || !sorting.Any() ? null : JsonSerializer.Serialize(sorting, JsonOptions)
        };

        public async Task<Dictionary<Guid, Guid>?> ResolveShareProductCategoriesAsync(
            IEnumerable<Guid> categoryIds,
            string targetUserId,
            string? authToken,
            CancellationToken cancellationToken)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);

            var model = new ProductCategoryResolveShareModel
            {
                CategoryIds = categoryIds.ToList(),
                TargetUserId = targetUserId
            };

            var controller = _config.Controllers![ProductCategoryRoutes.ControllerName];
            var response = await _httpClient.PostAsJsonAsync($"{controller}/resolveshare", model, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Dictionary<Guid, Guid>>(cancellationToken);
        }
    }
}
