using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class UnitService : IUnitService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _recipeBookApiConfig;

        public UnitService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _recipeBookApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);
        }

        public async Task<IList<UnitModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<UnitModel>?>(_recipeBookApiConfig.Endpoints[ApiEndpointNames.UnitApi]);
        }
    }
}
