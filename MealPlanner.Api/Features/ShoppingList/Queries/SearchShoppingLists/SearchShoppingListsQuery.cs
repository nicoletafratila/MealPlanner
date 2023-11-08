using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.SearchShoppingLists
{
    public class SearchShoppingListsQuery : IRequest<PagedList<ShoppingListModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
