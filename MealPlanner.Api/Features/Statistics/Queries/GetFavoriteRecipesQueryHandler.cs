using Common.Data.Entities;
using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries
{
    public class GetFavoriteRecipesQueryHandler(IMealPlanRepository mealPlanRepository) : IRequestHandler<GetFavoriteRecipesQuery, StatisticModel?>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;

        public async Task<StatisticModel?> Handle(GetFavoriteRecipesQuery request, CancellationToken cancellationToken)
        {
            var categoryId = int.Parse(request.CategoryId!);

            var data = new StatisticModel()
            {
                Title = "Favorite recipes",
                Data = new Dictionary<string, double>()
            };

            var mealPlanWithRecipes = await _mealPlanRepository.SearchByRecipeCategoryId(categoryId);
            foreach (var mealPlan in mealPlanWithRecipes!)
            {
                foreach (var recipe in mealPlan.MealPlanRecipes!.Where(i => i.Recipe!.RecipeCategoryId == categoryId))
                {
                    if (string.IsNullOrWhiteSpace(data.Label))
                        data.Label = recipe.Recipe!.RecipeCategory!.Name;

                    if (data.Data.ContainsKey(recipe.Recipe!.Name!))
                        data.Data[recipe.Recipe!.Name!]++;
                    else
                        data.Data[recipe.Recipe!.Name!] = 1;
                }
            }

            data.Data = data.Data.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return data;
        }
    }
}
