using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _mealPlannerApiConfig;

        public ShoppingListService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
        }

        public async Task<IList<ShoppingListModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ShoppingListModel>?>(_mealPlannerApiConfig.Endpoints[ApiEndPointNames.ShoppingListApi]);
        }

        public async Task<EditShoppingListModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditShoppingListModel?>($"{_mealPlannerApiConfig.Endpoints[ApiEndPointNames.ShoppingListApi]}/edit/{id}");
        }

        public async Task<EditShoppingListModel?> SaveShoppingListFromMealPlanAsync(int mealPlanId)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(mealPlanId), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_mealPlannerApiConfig.Endpoints[ApiEndPointNames.ShoppingListApi], modelJson);

            if (response.IsSuccessStatusCode)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<EditShoppingListModel?>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(_mealPlannerApiConfig.Endpoints[ApiEndPointNames.ShoppingListApi], modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_mealPlannerApiConfig.Endpoints[ApiEndPointNames.ShoppingListApi]}/{id}");
        }
    }
}
