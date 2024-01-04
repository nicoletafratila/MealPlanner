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
        private readonly IApiConfig _apiConfig;

        public ShoppingListService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
        }

        public async Task<EditShoppingListModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditShoppingListModel?>($"{_apiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/edit/{id}");
        }

        public async Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<ShoppingListModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<EditShoppingListModel?> MakeShoppingListAsync(MakeShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/makeShoppingList", modelJson);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<EditShoppingListModel?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiConfig.Endpoints[ApiEndpointNames.ShoppingListApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result!.Message;
        }

        public async Task<string?> UpdateAsync(EditShoppingListModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_apiConfig.Endpoints[ApiEndpointNames.ShoppingListApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result!.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiConfig.Endpoints[ApiEndpointNames.ShoppingListApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result!.Message;
        }
    }
}
