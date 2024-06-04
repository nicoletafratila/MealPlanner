using Common.Shared.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    public class SearchQueryHandler(IMealPlanRepository mealPlanRepository) : IRequestHandler<SearchQuery, IList<StatisticModel>>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;

        public async Task<IList<StatisticModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var result = new List<StatisticModel>();
            foreach (var category in request.Categories!)
            {
                var model = new StatisticModel
                {
                    Title = "Favorite " + category.Name,
                    Label = category.Name
                };

                var mealPlanWithProducts = await _mealPlanRepository.SearchByProductCategoryId(category.Id);
                foreach (var mealPlan in mealPlanWithProducts!)
                {
                    model.Data![mealPlan.Key.Name!] = !model.Data.TryGetValue(mealPlan.Key.Name!, out double value) ? 1 : ++value;
                }
                if (model.Data!.Any(i => i.Value == 1))
                {
                    model.Data = model.Data!.Where(i => i.Value > 1).ToDictionary(x => x.Key, x => x.Value);
                    model.Data!["Others"] = 1;
                }
                model.Data = model.Data?.OrderBy(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                result.Add(model);
            }

            return result;
        }
    }
}
