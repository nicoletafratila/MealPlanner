using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchRecipes
{
    public class SearchQuery : IRequest<IList<StatisticModel>>
    {
        public IList<int>? Categories { get; set; }
        public string? AuthToken { get; set; }
    }
}
