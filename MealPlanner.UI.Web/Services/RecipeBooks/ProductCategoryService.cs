using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public class ProductCategoryService(HttpClient httpClient, TokenProvider tokenProvider) : IProductCategoryService
    {
        private readonly IApiConfig _recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

        public async Task<ProductCategoryEditModel?> GetEditAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            return await httpClient.GetFromJsonAsync<ProductCategoryEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.ProductCategory]}/edit/{id}");
        }

        public async Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters<ProductCategoryModel>? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<ProductCategoryModel>.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonConvert.SerializeObject(queryParameters?.Filters),
                [nameof(QueryParameters<ProductCategoryModel>.Sorting)] = queryParameters == null || queryParameters?.Sorting == null ? null : JsonConvert.SerializeObject(queryParameters?.Sorting),
                [nameof(QueryParameters<ProductCategoryModel>.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters<ProductCategoryModel>.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.ProductCategory]}/search", query));
            return JsonConvert.DeserializeObject<PagedList<ProductCategoryModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> AddAsync(ProductCategoryEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PostAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.ProductCategory], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(ProductCategoryEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PutAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.ProductCategory], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.DeleteAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.ProductCategory]}/{id}");
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }
    }
}
