using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes
{
    public class GetFavoriteRecipesQueryHandler(IMealPlanRepository mealPlanRepository) : IRequestHandler<GetFavoriteRecipesQuery, StatisticModel?>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;

        public async Task<StatisticModel?> Handle(GetFavoriteRecipesQuery request, CancellationToken cancellationToken)
        {
            var categoryId = int.Parse(request.CategoryId!);
            var model = new StatisticModel()
            {
                Data = new Dictionary<string, double>()
            };

            var mealPlanWithRecipes = await _mealPlanRepository.SearchByRecipeCategoryId(categoryId);
            foreach (var mealPlan in mealPlanWithRecipes!)
            {
                foreach (var recipe in mealPlan.MealPlanRecipes!.Where(i => i.Recipe!.RecipeCategoryId == categoryId))
                {
                    if (string.IsNullOrWhiteSpace(model.Label))
                    {
                        model.Title = "Favorite " + recipe.Recipe!.RecipeCategory!.Name;
                        model.Label = recipe.Recipe!.RecipeCategory!.Name;
                    }

                    if (model.Data.ContainsKey(recipe.Recipe!.Name!))
                        model.Data[recipe.Recipe!.Name!]++;
                    else
                        model.Data[recipe.Recipe!.Name!] = 1;
                }
            }
            model.Data = model.Data.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return model;
        }
    }
}
