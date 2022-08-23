using RecipeBook.Shared.Models;
using System.Text.Json;

namespace MealPlanner.App.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly HttpClient _httpClient;
        private IQuantityCalculator _quantityCalculator;

        public ShoppingListService(HttpClient httpClient, IQuantityCalculator quantityCalculator)
        {
            _httpClient = httpClient;
            _quantityCalculator = quantityCalculator;
        }

        public async Task<ShoppingListModel> Get(int id)
        {
            var result = await JsonSerializer.DeserializeAsync<ShoppingListModel>
                    (await _httpClient.GetStreamAsync($"api/shoppinglist/{id}"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            result.Ingredients = _quantityCalculator.CalculateQuantities(result.Ingredients);
            return result;
        }
    }
}
