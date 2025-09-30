using System.Text;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class UnitService(HttpClient httpClient, TokenProvider tokenProvider) : IUnitService
    {
        private readonly IApiConfig _recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

        public async Task<UnitEditModel?> GetEditAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            return await httpClient.GetFromJsonAsync<UnitEditModel?>($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit]}/edit/{id}");
        }

        public async Task<PagedList<UnitModel>?> SearchAsync(QueryParameters<UnitModel>? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<UnitModel>.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonConvert.SerializeObject(queryParameters?.Filters),
                [nameof(QueryParameters<UnitModel>.Sorting)] = queryParameters == null || queryParameters?.Sorting == null ? null : JsonConvert.SerializeObject(queryParameters?.Sorting),
                [nameof(QueryParameters<UnitModel>.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters<UnitModel>.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit]}/search", query));
            return JsonConvert.DeserializeObject<PagedList<UnitModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> AddAsync(UnitEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PostAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(UnitEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PutAsync(_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.DeleteAsync($"{_recipeBookApiConfig?.Controllers![RecipeBookControllers.Unit]}/{id}");
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }
    }
}
