using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeCategoryService(HttpClient httpClient, IServiceProvider serviceProvider) : IRecipeCategoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<IList<RecipeCategoryModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<RecipeCategoryModel>?>(_apiConfig!.Endpoints![ApiEndpointNames.RecipeCategoryApi]);
        }
    }
}
