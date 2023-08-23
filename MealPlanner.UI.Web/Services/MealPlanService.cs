using Common.Constants;
using MealPlanner.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class MealPlanService : IMealPlanService
    {
        private readonly HttpClient _httpClient;

        public MealPlanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<MealPlanModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<MealPlanModel>?>(ApiNames.MealPlanApi);
        }

        public async Task<EditMealPlanModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditMealPlanModel?>($"{ApiNames.MealPlanApi}/{id}");
        }

        public async Task<EditMealPlanModel?> AddAsync(EditMealPlanModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.MealPlanApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditMealPlanModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditMealPlanModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.MealPlanApi, modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{ApiNames.MealPlanApi}/{id}");
        }
    }
}
