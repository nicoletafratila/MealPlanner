using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _recipeBookApiConfig;

        public ProductCategoryService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _recipeBookApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);
        }

        public async Task<IList<ProductCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ProductCategoryModel>>($"{_recipeBookApiConfig.Endpoints[ApiEndpointNames.ProductCategoryApi]}");
        }
    }
}
