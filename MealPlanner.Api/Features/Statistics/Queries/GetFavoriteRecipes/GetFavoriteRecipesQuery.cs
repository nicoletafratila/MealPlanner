using Common.Shared;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes
{
    public class GetFavoriteRecipesQuery : IRequest<IList<StatisticModel>>
    {
        public IList<RecipeCategoryModel>? Categories { get; set; }
    }
}
