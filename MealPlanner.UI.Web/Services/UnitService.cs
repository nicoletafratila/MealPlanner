using Common.Constants;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class UnitService : IUnitService
    {
        private readonly HttpClient _httpClient;

        public UnitService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<UnitModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<UnitModel>?>($"{ApiNames.UnitApi}");
        }
    }
}
