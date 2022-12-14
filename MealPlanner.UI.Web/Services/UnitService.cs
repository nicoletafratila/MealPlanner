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

        public async Task<IEnumerable<UnitModel>> GetAll()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UnitModel>>($"{ApiNames.UnitApi}");
        }
    }
}
