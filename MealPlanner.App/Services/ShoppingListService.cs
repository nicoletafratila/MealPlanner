using RecipeBook.Shared.Models;
using System.Text.Json;

namespace MealPlanner.App.Services
{
     public class ShoppingListService : IShoppingListService
    {
        private readonly HttpClient _httpClient;

        public ShoppingListService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ShoppingListModel> Get(int id)
        {
            return await JsonSerializer.DeserializeAsync<ShoppingListModel>
                    (await _httpClient.GetStreamAsync($"api/shoppinglist/{id}"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }
    }
}
