using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.Search
{
    /// <summary>
    /// Query to search shops with pagination, filtering, and sorting.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<ShopModel>>
    {
        /// <summary>
        /// Pagination, filtering, and sorting parameters for shops.
        /// </summary>
        public QueryParameters<ShopModel>? QueryParameters { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(QueryParameters<ShopModel> queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}