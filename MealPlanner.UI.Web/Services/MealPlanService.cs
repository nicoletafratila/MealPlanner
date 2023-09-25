using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class MealPlanService : IMealPlanService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _mealPlannerApiConfig;

        public MealPlanService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
        }

        public async Task<IList<MealPlanModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<MealPlanModel>?>(_mealPlannerApiConfig.Endpoints[ApiEndPointNames.MealPlanApi]);
        }

        public async Task<EditMealPlanModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditMealPlanModel?>($"{_mealPlannerApiConfig.Endpoints[ApiEndPointNames.MealPlanApi]}/edit/{id}");
        }

        public async Task<EditMealPlanModel?> AddAsync(EditMealPlanModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_mealPlannerApiConfig.Endpoints[ApiEndPointNames.MealPlanApi], modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditMealPlanModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditMealPlanModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(_mealPlannerApiConfig.Endpoints[ApiEndPointNames.MealPlanApi], modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_mealPlannerApiConfig.Endpoints[ApiEndPointNames.MealPlanApi]}/{id}");
        }
    }
}
