using System.Text;
using System.Text.Json;
using BlazorBootstrap;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class RecipeService(HttpClient httpClient) : IRecipeService
    {
        private readonly IApiConfig _recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

        public async Task<RecipeModel?> GetByIdAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<RecipeModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe]}/{id}");
        }

        public async Task<RecipeEditModel?> GetEditAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<RecipeEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe]}/edit/{id}");
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int recipeId, int shopId)
        {
            return await httpClient.GetFromJsonAsync<IList<ShoppingListProductEditModel>?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe]}/shoppingListProducts/{recipeId}/{shopId}");
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

            var a = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<RecipeModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(RecipeEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(RecipeEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Recipe]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}