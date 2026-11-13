using Common.Models;
using Common.Pagination;
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

        /// <summary>
        /// Optional filters to apply to the underlying recipe categories, keeping the result consistent with the caller's current filter state.
        /// </summary>
        public IEnumerable<FilterItem>? Filters { get; set; }

        /// <summary>
        /// Optional sorting to apply to the underlying recipe categories, keeping the result consistent with the caller's current sort state.
        /// </summary>
        public IEnumerable<SortingModel>? Sorting { get; set; }

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