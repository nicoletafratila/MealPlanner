﻿using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Data.DataContext;
using Common.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class StatisticsService(HttpClient httpClient) : IStatisticsService
    {
        private readonly IApiConfig _mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IList<RecipeCategoryModel> categories)
        {
            var query = new Dictionary<string, string?>
            {
                ["categories"] = string.Join(",", categories.Select(i => i.Id + "|" + i.Name!))
            };
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Statistics]}/favoriterecipes", query!));
            return JsonSerializer.Deserialize<IList<StatisticModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IList<ProductCategoryModel> categories)
        {
            var query = new Dictionary<string, string?>
            {
                ["categories"] = string.Join(",", categories.Select(i => i.Id + "|" + i.Name!))
            };
            var response = await httpClient.GetAsync(QueryHelpers.AddQueryString($"{_mealPlannerApiConfig?.Controllers![MealPlannerControllers.Statistics]}/favoriteproducts", query!));
            return JsonSerializer.Deserialize<IList<StatisticModel>?>(await response.Content.ReadAsStringAsync());
        }
    }
}
