using System.Text;
using BlazorBootstrap;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Services
{
    public class MealPlanService(HttpClient httpClient, TokenProvider tokenProvider) : IMealPlanService
    {
        private readonly IApiConfig _mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

        public async Task<MealPlanEditModel?> GetEditAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            return await httpClient.GetFromJsonAsync<MealPlanEditModel?>($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan]}/edit/{id}");
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int mealPlanId, int shopId)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            return await httpClient.GetFromJsonAsync<IList<ShoppingListProductEditModel>?>($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan]}/shoppingListProducts/{mealPlanId}/{shopId}");
        }

        public async Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonConvert.SerializeObject(queryParameters?.Filters),
                [nameof(QueryParameters.SortString)] = queryParameters == null ? "Name" : queryParameters?.SortString?.ToString(),
                [nameof(QueryParameters.SortDirection)] = queryParameters == null ? SortDirection.Ascending.ToString() : queryParameters.SortDirection.ToString(),
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan]}/search", query));
            return JsonConvert.DeserializeObject<PagedList<MealPlanModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> AddAsync(MealPlanEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PostAsync(_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(MealPlanEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.PutAsync(_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var response = await httpClient.DeleteAsync($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.MealPlan]}/{id}");
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }
    }
}
