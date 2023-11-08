using Common.Api;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
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

        public async Task<EditShoppingListModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditShoppingListModel?>($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/edit/{id}");
        }

        public async Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ShoppingListModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<EditShoppingListModel?> SaveShoppingListFromMealPlanAsync(int mealPlanId)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(mealPlanId), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_mealPlannerApiConfig.Endpoints[ApiEndpointNames.ShoppingListApi], modelJson);

            if (response.IsSuccessStatusCode)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<EditShoppingListModel?>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(_mealPlannerApiConfig.Endpoints[ApiEndpointNames.ShoppingListApi], modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/{id}");
        }
    }
}
