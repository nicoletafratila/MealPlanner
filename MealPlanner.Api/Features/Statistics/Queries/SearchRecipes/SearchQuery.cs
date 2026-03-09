using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchRecipes
{
    /// <summary>
    /// Query to retrieve favorite recipe statistics, optionally filtered by recipe category IDs.
    /// </summary>
    public class SearchQuery : IRequest<IList<StatisticModel>>
    {
        /// <summary>
        /// Comma-separated list of recipe category IDs to filter by (e.g. "1,2,3").
        /// Optional; if null/empty the handler can treat it as "all categories".
        /// </summary>
        public string? CategoryIds { get; set; }

        /// <summary>
        /// Optional bearer token used for downstream API calls, if needed.
        /// </summary>
        public string? AuthToken { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(string? categoryIds, string? authToken)
        {
            CategoryIds = categoryIds;
            AuthToken = authToken;
        }
    }
}