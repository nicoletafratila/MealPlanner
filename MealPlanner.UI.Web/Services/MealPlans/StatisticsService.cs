using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public class StatisticsService(HttpClient httpClient, TokenProvider tokenProvider) : IStatisticsService
    {
        private readonly IApiConfig _mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IList<RecipeCategoryModel> categories)
        {
            var query = new Dictionary<string, string?>
            {
                ["categoryIds"] = string.Join(",", categories.Select(i => i.Id.ToString()))
            };

            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var url = QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Statistics]}/favoriterecipes", query);
            return await httpClient.GetFromJsonAsync<IList<StatisticModel>>(url);
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IList<ProductCategoryModel> categories)
        {
            var query = new Dictionary<string, string?>
            {
                ["categoryIds"] = string.Join(",", categories.Select(i => i.Id.ToString()))
            };

            await httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);
            var url = QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Statistics]}/favoriteproducts", query);
            return await httpClient.GetFromJsonAsync<IList<StatisticModel>>(url);
        }
    }
}
