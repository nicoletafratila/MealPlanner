using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes
{
    public class GetFavoriteRecipesQueryHandler(IMealPlanRepository mealPlanRepository) : IRequestHandler<GetFavoriteRecipesQuery, IList<StatisticModel>>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;

        public async Task<IList<StatisticModel>> Handle(GetFavoriteRecipesQuery request, CancellationToken cancellationToken)
        {
            var result = new List<StatisticModel>();
            foreach (var category in request.Categories!)
            {
                var model = new StatisticModel
                {
                    Title = "Favorite " + category.Name,
                    Label = category.Name
                };

                var mealPlanWithRecipes = await _mealPlanRepository.SearchByRecipeCategoryId(category.Id);
                foreach (var mealPlan in mealPlanWithRecipes!)
                {
                    if (mealPlan.Recipe?.RecipeCategoryId == category.Id)
                    {
                        if (model.Data!.ContainsKey(mealPlan.Recipe?.Name!))
                            model.Data[mealPlan.Recipe?.Name!]++;
                        else
                            model.Data[mealPlan.Recipe?.Name!] = 1;
                    }
                }
                model.Data = model.Data?.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                result.Add(model);
            }

            return result;
        }
    }
}
