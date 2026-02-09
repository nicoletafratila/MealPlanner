using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    public class SearchQuery : IRequest<IList<StatisticModel>>
    {
        public string? CategoryIds { get; set; }
        public string? AuthToken { get; set; }
    }
}
