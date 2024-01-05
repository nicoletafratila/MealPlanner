using System.Text;
using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ProductService(HttpClient httpClient, IServiceProvider serviceProvider) : IProductService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<EditProductModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditProductModel?>($"{_apiConfig!.Endpoints![ApiEndpointNames.ProductApi]}/edit/{id}");
        }

        public async Task<PagedList<ProductModel>?> SearchAsync(string? categoryId = null, QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                ["CategoryId"] = categoryId,
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig!.Endpoints![ApiEndpointNames.ProductApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ProductModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(EditProductModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiConfig!.Endpoints![ApiEndpointNames.ProductApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result!.Message;
        }

        public async Task<string?> UpdateAsync(EditProductModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_apiConfig!.Endpoints![ApiEndpointNames.ProductApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result!.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiConfig!.Endpoints![ApiEndpointNames.ProductApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result!.Message;
        }
    }
}
