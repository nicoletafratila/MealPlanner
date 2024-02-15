using Common.Api;
using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class UnitService(HttpClient httpClient, IServiceProvider serviceProvider) : IUnitService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<IList<UnitModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<UnitModel>?>(_apiConfig?.Endpoints![ApiEndpointNames.UnitApi]);
        }
    }
}
