using System.Text;
using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ProductCategoryService(HttpClient httpClient, IServiceProvider serviceProvider) : IProductCategoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<EditProductCategoryModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditProductCategoryModel?>($"{_apiConfig?.Endpoints![ApiEndpointNames.ProductCategoryApi]}/edit/{id}");
        }

        public async Task<IList<ProductCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>($"{_apiConfig?.Endpoints![ApiEndpointNames.ProductCategoryApi]}");
        }

        public async Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig?.Endpoints![ApiEndpointNames.ProductCategoryApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ProductCategoryModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(EditProductCategoryModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiConfig?.Endpoints![ApiEndpointNames.ProductCategoryApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(EditProductCategoryModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_apiConfig?.Endpoints![ApiEndpointNames.ProductCategoryApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiConfig?.Endpoints![ApiEndpointNames.ProductCategoryApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}
