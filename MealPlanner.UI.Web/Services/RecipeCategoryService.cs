using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeCategoryService : IRecipeCategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _recipeBookApiConfig;

        public RecipeCategoryService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _recipeBookApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);
        }

        public async Task<IList<RecipeCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeCategoryModel>?>(_recipeBookApiConfig.Endpoints[ApiEndPointNames.RecipeCategoryApi]);
        }
    }
}
