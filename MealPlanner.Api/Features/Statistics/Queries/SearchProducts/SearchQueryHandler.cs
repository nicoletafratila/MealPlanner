using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    public class SearchQueryHandler(IMealPlanRepository mealPlanRepository, RecipeBookApiConfig recipeBookApiConfig) : IRequestHandler<SearchQuery, IList<StatisticModel>>
    {
        public async Task<IList<StatisticModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var result = new List<StatisticModel>();
            if (string.IsNullOrWhiteSpace(request.CategoryIds))
            {
                return result;
            }

            var categories = await LoadCategoriesAsync(request.CategoryIds!, request.AuthToken, cancellationToken);
            if (categories is null || !categories.Any())
            {
                return result;
            }

            var categoryIds = string.IsNullOrWhiteSpace(request.CategoryIds)
               ? new List<int>()
               : request.CategoryIds
                   .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Select(part => int.TryParse(part, out var id) ? (int?)id : null)
                   .Where(id => id.HasValue)
                   .Select(id => id!.Value)
                   .ToList();

            var mealPlans = await mealPlanRepository.SearchByProductCategoryIdsAsync(categoryIds);

            var mealPlansByCategory = mealPlans!
                .Where(mp => mp.Key != null)
                .GroupBy(mp => mp.Key!.ProductCategoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var category in categories)
            {
                var model = new StatisticModel
                {
                    Title = category.Name,
                    Label = category.Name
                };

                if (!mealPlansByCategory.TryGetValue(category.Id, out var mealPlanWithProducts))
                {
                    result.Add(model);
                    continue;
                }

                foreach (var mealPlan in mealPlanWithProducts)
                {
                    var Product = mealPlan.Key!;
                    var ProductName = Product.Name!;

                    if (!model.Data!.TryGetValue(ProductName, out var value))
                    {
                        model.Data[ProductName] = 1;
                    }
                    else
                    {
                        model.Data[ProductName] = value + 1;
                    }
                }

                if (model.Data!.Count > 0)
                {
                    double others = 0;
                    var filtered = new Dictionary<string, double?>();

                    foreach (var kvp in model.Data)
                    {
                        var v = kvp.Value ?? 0;
                        if (v > 1)
                        {
                            filtered[kvp.Key] = v;
                        }
                        else
                        {
                            others += v;
                        }
                    }

                    if (others > 0)
                    {
                        filtered["Others"] = others;
                    }

                    model.Data = filtered
                        .OrderBy(x => x.Value)
                        .ThenBy(x => x.Key)
                        .ToDictionary(x => x.Key, x => x.Value);
                }

                result.Add(model);
            }

            return result;
        }

        private async Task<IList<ProductCategoryModel>?> LoadCategoriesAsync(string categoryIds, string? authToken, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.EnsureAuthorizationHeader(authToken);
            client.BaseAddress = recipeBookApiConfig?.BaseUrl;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var query = new Dictionary<string, string?>
            {
                ["categoryIds"] = categoryIds
            };
            var controller = recipeBookApiConfig!.Controllers![RecipeBookControllers.ProductCategory];
            var url = QueryHelpers.AddQueryString($"{controller}/searchbycategories", query);
            return await client.GetFromJsonAsync<IList<ProductCategoryModel>>(url, cancellationToken);
        }
    }
}
