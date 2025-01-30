using System.Text;
using System.Text.Json;
using BlazorBootstrap;
using Common.Api;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeService(HttpClient httpClient, IServiceProvider serviceProvider) : IRecipeService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<RecipeModel?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeModel?>($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi]}/{id}");
        }

        public async Task<RecipeEditModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RecipeEditModel?>($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi]}/edit/{id}");
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProducts(int recipeId, int shopId)
        {
            return await _httpClient.GetFromJsonAsync<IList<ShoppingListProductEditModel>?>($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi]}/shoppingListProducts/{recipeId}/{shopId}");
        }

        public async Task<PagedList<RecipeModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonSerializer.Serialize(queryParameters?.Filters),
                [nameof(QueryParameters.SortString)] = queryParameters == null ? "Name" : queryParameters?.SortString?.ToString(),
                [nameof(QueryParameters.SortDirection)] = queryParameters == null ? SortDirection.Ascending.ToString() : queryParameters.SortDirection.ToString(),
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<RecipeModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(RecipeEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(RecipeEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiConfig?.Endpoints![ApiEndpointNames.RecipeApi]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}