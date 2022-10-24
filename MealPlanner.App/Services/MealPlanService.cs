using MealPlanner.Shared.Models;

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
            return await _httpClient.GetFromJsonAsync<IEnumerable<MealPlanModel>>($"api/mealplan");
        }

        public async Task<EditMealPlanModel> Get(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditMealPlanModel>($"api/mealplan/{id}");
        }
    }
}
