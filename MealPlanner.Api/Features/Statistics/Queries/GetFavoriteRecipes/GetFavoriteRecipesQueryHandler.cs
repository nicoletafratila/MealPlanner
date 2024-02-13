using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes
{
    public class GetFavoriteRecipesQueryHandler(IMealPlanRepository mealPlanRepository, IServiceProvider serviceProvider) : IRequestHandler<GetFavoriteRecipesQuery, IList<StatisticModel>>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
        private readonly IApiConfig _recipeApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<IList<StatisticModel>> Handle(GetFavoriteRecipesQuery request, CancellationToken cancellationToken)
        {
            IList<RecipeCategoryModel>? categories = new List<RecipeCategoryModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = _recipeApiConfig!.BaseUrl;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                categories = await client.GetFromJsonAsync<IList<RecipeCategoryModel>>($"{_recipeApiConfig!.Endpoints![ApiEndpointNames.RecipeCategoryApi]}", cancellationToken: cancellationToken);
            }

            var result = new List<StatisticModel>();
            foreach (var category in categories!)
            {
                var model = new StatisticModel();
                model.Title = "Favorite " + category!.Name;
                model.Label = category!.Name;

                var mealPlanWithRecipes = await _mealPlanRepository.SearchByRecipeCategoryId(category.Id);
                foreach (var mealPlan in mealPlanWithRecipes!)
                {
                    foreach (var recipe in mealPlan.MealPlanRecipes!.Where(i => i.Recipe!.RecipeCategoryId == category.Id))
                    {
                        if (string.IsNullOrWhiteSpace(model.Label))
                        {

                        }

                        if (model.Data!.ContainsKey(recipe.Recipe!.Name!))
                            model.Data[recipe.Recipe!.Name!]++;
                        else
                            model.Data[recipe.Recipe!.Name!] = 1;
                    }
                }
                model.Data = model.Data!.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                result.Add(model);
            }

            return result;
        }
    }
}
