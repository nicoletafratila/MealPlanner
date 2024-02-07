using Common.Shared;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries
{
    public class GetFavoriteRecipesQuery : IRequest<StatisticModel>
    {
        public string? CategoryId { get; set; }
    }
}
