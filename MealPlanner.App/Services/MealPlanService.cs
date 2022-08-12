using RecipeBook.Shared.Models;
using System.Text.Json;

namespace MealPlanner.App.Services
{
    public class MealPlanService : IMealPlanService
    {
        private readonly HttpClient _httpClient;

        public MealPlanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<MealPlanModel>> GetAll()
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<MealPlanModel>>
                    (await _httpClient.GetStreamAsync($"api/mealplan"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        public async Task<MealPlanModel> Get(int id)
        {
            return await JsonSerializer.DeserializeAsync<MealPlanModel>
                    (await _httpClient.GetStreamAsync($"api/mealplan/{id}"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }
    }
}
