using Common.Models;
using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    public class SearchQueryHandler(
        IMealPlanRepository mealPlanRepository,
        IRecipeBookClient recipeBookClient)
        : IRequestHandler<SearchQuery, IList<StatisticModel>>
    {
        private readonly IMealPlanRepository _mealPlanRepository =
            mealPlanRepository ?? throw new ArgumentNullException(nameof(mealPlanRepository));

        private readonly IRecipeBookClient _recipeBookClient =
            recipeBookClient ?? throw new ArgumentNullException(nameof(recipeBookClient));

        public async Task<IList<StatisticModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var result = new List<StatisticModel>();

            if (string.IsNullOrWhiteSpace(request.CategoryIds))
            {
                return result;
            }

            var categoryIds = ParseCategoryIds(request.CategoryIds);
            if (categoryIds.Count == 0)
            {
                return result;
            }

            var categories = await _recipeBookClient.GetProductCategoriesAsync(request.CategoryIds!, request.AuthToken, cancellationToken);

            if (categories is null || !categories.Any())
            {
                return result;
            }

            var mealPlans = await _mealPlanRepository.SearchByProductCategoryIdsAsync(categoryIds, cancellationToken);

            var mealPlansByCategory = mealPlans
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

                foreach (var pair in mealPlanWithProducts)
                {
                    var product = pair.Key!;
                    var productName = product.Name ?? string.Empty;

                    if (!model.Data!.TryGetValue(productName, out var value))
                    {
                        model.Data[productName] = 1;
                    }
                    else
                    {
                        model.Data[productName] = value + 1;
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
                        .OrderByDescending(x => x.Value)
                        .ThenBy(x => x.Key)
                        .ToDictionary(x => x.Key, x => x.Value);
                }

                result.Add(model);
            }

            return result;
        }

        private static List<int> ParseCategoryIds(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return [];

            return ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var id) ? (int?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }
    }
}