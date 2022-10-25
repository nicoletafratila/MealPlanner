using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
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
            var result = await _httpClient.GetFromJsonAsync<ShoppingListModel>($"api/shoppinglist/{id}");
            result.Ingredients = _quantityCalculator.CalculateQuantities(result.Ingredients);
            return result;
        }
    }
}
