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

        public async Task<IList<ProductCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>($"{_apiConfig!.Endpoints![ApiEndpointNames.ProductCategoryApi]}");
        }

        public async Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig!.Endpoints![ApiEndpointNames.ProductCategoryApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ProductCategoryModel>?>(await response.Content.ReadAsStringAsync());
        }
    }
}
