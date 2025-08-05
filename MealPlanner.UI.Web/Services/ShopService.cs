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
    public class ShopService(HttpClient httpClient) : IShopService
    {
        private readonly IApiConfig _mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

        public async Task<ShopEditModel?> GetEditAsync(int id)
        {
            return await httpClient.GetFromJsonAsync<ShopEditModel?>($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Shop]}/edit/{id}");
        }

        public async Task<PagedList<ShopModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.Filters)] = queryParameters == null || queryParameters?.Filters == null ? null : JsonConvert.SerializeObject(queryParameters?.Filters),
                [nameof(QueryParameters.SortString)] = queryParameters == null ? "Name" : queryParameters?.SortString?.ToString(),
                [nameof(QueryParameters.SortDirection)] = queryParameters == null ? SortDirection.Ascending.ToString() : queryParameters.SortDirection.ToString(),
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Shop]}/search", query));
            return JsonConvert.DeserializeObject<PagedList<ShopModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> AddAsync(ShopEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Shop], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> UpdateAsync(ShopEditModel model)
        {
            var modelJson = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Shop], modelJson);
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            var response = await httpClient.DeleteAsync($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Shop]}/{id}");
            return JsonConvert.DeserializeObject<CommandResponse?>(await response.Content.ReadAsStringAsync());
        }
    }
}
