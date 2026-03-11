using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{
    /// <summary>
    /// Query to search meal plans with pagination, filtering, and sorting.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<MealPlanModel>>
    {
        /// <summary>
        /// Pagination, filtering, and sorting parameters for meal plans.
        /// </summary>
        public QueryParameters<MealPlanModel>? QueryParameters { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(QueryParameters<MealPlanModel> queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}