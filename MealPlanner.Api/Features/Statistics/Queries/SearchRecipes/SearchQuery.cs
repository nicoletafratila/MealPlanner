using Common.Shared;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchRecipes
{
    public class SearchQuery : IRequest<IList<StatisticModel>>
    {
        public IList<RecipeCategoryModel>? Categories { get; set; }
    }
}
