using Common.Constants;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
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
            return await _httpClient.GetFromJsonAsync<ShoppingListModel>($"{ApiNames.ShoppingListApi}/{id}");
        }
    }
}
