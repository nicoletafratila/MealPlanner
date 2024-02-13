using Common.Shared;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes
{
    public class GetFavoriteRecipesQuery : IRequest<IList<StatisticModel>>
    {
    }
}
