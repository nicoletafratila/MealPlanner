using Common.Constants;
using MealPlanner.Shared.Models;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly HttpClient _httpClient;

        public ShoppingListService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<ShoppingListModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ShoppingListModel>?>($"{ApiNames.ShoppingListApi}");
        }

        public async Task<EditShoppingListModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditShoppingListModel?>($"{ApiNames.ShoppingListApi}/{id}");
        }

        public async Task<EditShoppingListModel?> SaveShoppingListFromMealPlanAsync(int mealPlanId)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(mealPlanId), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.ShoppingListApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<EditShoppingListModel?>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.ShoppingListApi, modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{ApiNames.ShoppingListApi}/{id}");
        }
    }
}
