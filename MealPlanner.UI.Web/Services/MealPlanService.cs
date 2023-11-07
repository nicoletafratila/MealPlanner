using Common.Api;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace MealPlanner.UI.Web.Services
{
    public class MealPlanService : IMealPlanService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _mealPlannerApiConfig;

        public MealPlanService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
        }

        public async Task<EditMealPlanModel?> GetEditAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EditMealPlanModel?>($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi]}/edit/{id}");
        }

        public async Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters? queryParameters = null)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters.PageSize)] = queryParameters == null ? int.MaxValue.ToString() : queryParameters.PageSize.ToString(),
                [nameof(QueryParameters.PageNumber)] = queryParameters == null ? "1" : queryParameters.PageNumber.ToString()
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi]}/search", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PagedList<MealPlanModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<EditMealPlanModel?> AddAsync(EditMealPlanModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi], modelJson);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<EditMealPlanModel?>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }

        public async Task UpdateAsync(EditMealPlanModel model)
        {
            var modelJson = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            await _httpClient.PutAsync(_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi], modelJson);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi]}/{id}");
        }
    }
}
