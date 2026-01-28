using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchRecipes
{
    public class SearchQueryHandler(IMealPlanRepository mealPlanRepository, RecipeBookApiConfig recipeBookApiConfig) : IRequestHandler<SearchQuery, IList<StatisticModel>>
    {
        public async Task<IList<StatisticModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var result = new List<StatisticModel>();

            if (request.Categories == null || request.Categories.Count == 0)
            {
                return result;
            }

            var categoryIds = request.Categories
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (categoryIds.Count == 0)
            {
                return result;
            }

            var categories = await LoadCategoriesAsync(categoryIds, request.AuthToken, cancellationToken);
            if (categories.Count == 0)
            {
                return result;
            }

            var mealPlans = await mealPlanRepository.SearchByRecipeCategoryIdsAsync(categoryIds);

            var mealPlansByCategory = mealPlans
                .Where(mp => mp.Recipe != null)
                .GroupBy(mp => mp.Recipe!.RecipeCategoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var category in categories)
            {
                var model = new StatisticModel
                {
                    Title = category.Name,
                    Label = category.Name
                };

                if (!mealPlansByCategory.TryGetValue(category.Id, out var mealPlanWithRecipes))
                {
                    result.Add(model);
                    continue;
                }

                foreach (var mealPlan in mealPlanWithRecipes)
                {
                    var recipe = mealPlan.Recipe!;
                    var recipeName = recipe.Name!;

                    if (!model.Data!.TryGetValue(recipeName, out var value))
                    {
                        model.Data[recipeName] = 1;
                    }
                    else
                    {
                        model.Data[recipeName] = value + 1;
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

        private async Task<List<RecipeCategoryModel>> LoadCategoriesAsync(List<int> categoryIds, string? authToken, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                client.EnsureAuthorizationHeader(authToken);
                client.BaseAddress = recipeBookApiConfig?.BaseUrl;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var query = new Dictionary<string, string?>
                {
                    [nameof(QueryParameters<RecipeCategoryModel>.Filters)] = null,
                    [nameof(QueryParameters<RecipeCategoryModel>.Sorting)] = null,
                    [nameof(QueryParameters<RecipeCategoryModel>.PageSize)] = int.MaxValue.ToString(),
                    [nameof(QueryParameters<RecipeCategoryModel>.PageNumber)] = "1",
                };

                var controller = recipeBookApiConfig!.Controllers![RecipeBookControllers.RecipeCategory];
                var url = QueryHelpers.AddQueryString($"{controller}/search", query);

                var allCategories = await client.GetFromJsonAsync<PagedList<RecipeCategoryModel>>(url, cancellationToken);

                if (allCategories?.Items == null || allCategories.Items.Count == 0)
                {
                    return new List<RecipeCategoryModel>();
                }

                var requestedSet = new HashSet<int>(categoryIds);
                return allCategories.Items
                    .Where(item => requestedSet.Contains(item.Id))
                    .ToList();
            }
        }
    }
}
