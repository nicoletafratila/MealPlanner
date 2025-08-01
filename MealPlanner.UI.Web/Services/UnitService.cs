using System.Text;
using System.Text.Json;
using BlazorBootstrap;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class UnitService(HttpClient httpClient) : IUnitService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>(); 

        public async Task<UnitEditModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<UnitEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit]}/edit/{id}");
        }

        public async Task<PagedList<UnitModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonSerializer.Serialize(queryParameters?.Filters),
                [nameof(QueryParameters.SortString)] = queryParameters == null ? "Name" : queryParameters?.SortString?.ToString(),
                [nameof(QueryParameters.SortDirection)] = queryParameters == null ? SortDirection.Ascending.ToString() : queryParameters.SortDirection.ToString(),
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<UnitModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string?> AddAsync(UnitEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> UpdateAsync(UnitEditModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit], modelJson);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit]}/{id}");
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
            {
                Message = string.Empty
            });
            return result?.Message;
        }
    }
}
