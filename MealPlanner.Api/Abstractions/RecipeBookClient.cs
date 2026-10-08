using Common.Http;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Abstractions
{
    public class RecipeBookClient(HttpClient httpClient, RecipeBookClientConfig config) : IRecipeBookClient
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly RecipeBookClientConfig _config = config ?? throw new ArgumentNullException(nameof(config));

        public async Task<IList<RecipeCategoryModel>?> GetRecipeCategoriesAsync(
            string categoryIds,
            string? authToken,
            CancellationToken cancellationToken)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);

            var query = new Dictionary<string, string?>
            {
                ["categoryIds"] = categoryIds
            };

            var controller = _config.Controllers![RecipeBookControllers.RecipeCategory];
            var url = QueryHelpers.AddQueryString($"{controller}/searchbycategories", query);

            return await _httpClient.GetFromJsonAsync<IList<RecipeCategoryModel>>(url, cancellationToken);
        }

        public async Task<IList<ProductCategoryModel>?> GetProductCategoriesAsync(
            string categoryIds,
            string? authToken,
            CancellationToken cancellationToken)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);

            var query = new Dictionary<string, string?>
            {
                ["categoryIds"] = categoryIds
            };

            var controller = _config.Controllers![RecipeBookControllers.ProductCategory];
            var url = QueryHelpers.AddQueryString($"{controller}/searchbycategories", query);

            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>(url, cancellationToken);
        }

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

            var controller = _config.Controllers![RecipeBookControllers.ProductCategory];
            var response = await _httpClient.PostAsJsonAsync($"{controller}/resolveshare", model, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Dictionary<Guid, Guid>>(cancellationToken);
        }
    }
}
