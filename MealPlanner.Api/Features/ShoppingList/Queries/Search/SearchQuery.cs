using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.Search
{
    /// <summary>
    /// Query to search shopping lists with pagination, filtering, and sorting.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<ShoppingListModel>>
    {
        /// <summary>
        /// Pagination, filtering, and sorting parameters for shopping lists.
        /// </summary>
        public QueryParameters<ShoppingListModel>? QueryParameters { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(QueryParameters<ShoppingListModel> queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}