using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;

namespace RecipeBook.Api.Abstractions
{
    public class MealPlannerClient(HttpClient httpClient, IApiConfig apiConfig) : IMealPlannerClient
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly IApiConfig _apiConfig = apiConfig ?? throw new ArgumentNullException(nameof(apiConfig));

        public async Task<ShopEditModel?> GetShopAsync(int shopId, string? authToken, CancellationToken cancellationToken)
        {
            if (shopId <= 0)
                throw new ArgumentOutOfRangeException(nameof(shopId), "shopId must be greater than zero.");

            ConfigureClient(authToken);

            var controllerPath = _apiConfig.Controllers![MealPlannerControllers.Shop];
            var url = $"{controllerPath}/edit?id={shopId}";

            return await _httpClient.GetFromJsonAsync<ShopEditModel>(url, cancellationToken);
        }

        public async Task<IList<MealPlanModel>?> GetMealPlansByRecipeIdAsync(
            int recipeId,
            string? authToken,
            CancellationToken cancellationToken)
        {
            if (recipeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(recipeId), "recipeId must be greater than zero.");

            ConfigureClient(authToken);

            var controllerPath = _apiConfig.Controllers![MealPlannerControllers.MealPlan];
            var url = $"{controllerPath}/searchbyid?id={recipeId}";

            return await _httpClient.GetFromJsonAsync<IList<MealPlanModel>>(url, cancellationToken);
        }

        private void ConfigureClient(string? authToken)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);
            _httpClient.BaseAddress = _apiConfig.BaseUrl;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}