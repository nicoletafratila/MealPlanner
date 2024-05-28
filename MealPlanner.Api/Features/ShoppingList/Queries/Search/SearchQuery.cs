using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<ShoppingListModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
