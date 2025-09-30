using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<ShopModel>>
    {
        public QueryParameters<ShopModel>? QueryParameters { get; set; }
    }
}
