using Common.Api;
using Common.Constants;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Abstractions
{
    public class RecipeBookClient(HttpClient httpClient, RecipeBookApiConfig config) : IRecipeBookClient
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly RecipeBookApiConfig _config = config ?? throw new ArgumentNullException(nameof(config));

        public async Task<IList<RecipeCategoryModel>?> GetCategoriesAsync(
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
    }
}
