using System.Net.Http.Headers;using Common.Http; using MealPlanner.Shared.Constants; using MealPlanner.Shared.Models;

namespace RecipeBook.Api.Abstractions
{
    public class MealPlannerClient(HttpClient httpClient, MealPlannerClientConfig apiConfig) : IMealPlannerClient
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly MealPlannerClientConfig _apiConfig = apiConfig ?? throw new ArgumentNullException(nameof(apiConfig));

        public async Task<ShopEditModel?> GetShopAsync(Guid shopId, string? authToken, CancellationToken cancellationToken)
        {
            if (shopId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(shopId), "shopId must not be empty.");

            ConfigureClient(authToken);

            var controllerPath = _apiConfig.Controllers![MealPlannerControllers.Shop];
            var url = $"{controllerPath}/edit?id={shopId}";

            return await _httpClient.GetFromJsonAsync<ShopEditModel>(url, cancellationToken);
        }

        public async Task<IList<MealPlanModel>?> GetMealPlansByRecipeIdAsync(
            Guid recipeId,
            string? authToken,
            CancellationToken cancellationToken)
        {
            if (recipeId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(recipeId), "recipeId must not be empty.");

            ConfigureClient(authToken);

            var controllerPath = _apiConfig.Controllers![MealPlannerControllers.MealPlan];
            var url = $"{controllerPath}/searchbyid?id={recipeId}";

            return await _httpClient.GetFromJsonAsync<IList<MealPlanModel>>(url, cancellationToken);
        }

        private void ConfigureClient(string? authToken)
        {
            _httpClient.EnsureAuthorizationHeader(authToken);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}