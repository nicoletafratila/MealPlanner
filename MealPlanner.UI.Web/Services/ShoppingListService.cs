using Common.Constants;
using MealPlanner.Shared.Models;
using System.Text.Json;
using System.Text;

namespace MealPlanner.UI.Web.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly HttpClient _httpClient;

        public ShoppingListService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ShoppingListModel?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ShoppingListModel?>($"{ApiNames.ShoppingListApi}/{id}");
        }

        public async Task<EditShoppingListModel?> AddAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiNames.ShoppingListApi, modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditShoppingListModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(ApiNames.ShoppingListApi, modelJson);
        }
    }
}
