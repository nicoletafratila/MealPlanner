using Common.Api;
using Common.Constants;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _recipeBookApiConfig;

        public ProductService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _recipeBookApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);
        }

        public async Task<EditProductModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditProductModel?>($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.ProductApi]}/edit/{id}");
        }

        public async Task<PagedList<ProductModel>?> SearchAsync(string? categoryId = null, QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                ["CategoryId"] = categoryId,
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.ProductApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ProductModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<EditProductModel?> AddAsync(EditProductModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_recipeBookApiConfig.Endpoints[ApiEndpointNames.ProductApi], modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditProductModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditProductModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(_recipeBookApiConfig.Endpoints[ApiEndpointNames.ProductApi], modelJson);
        }

        public async Task<string> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.ProductApi]}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }
    }
}
