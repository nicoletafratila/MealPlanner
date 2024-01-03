using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _apiConfig;

        public ProductCategoryService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);
        }

        public async Task<IList<ProductCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>($"{_apiConfig.Endpoints[ApiEndpointNames.ProductCategoryApi]}");
        }
    }
}
