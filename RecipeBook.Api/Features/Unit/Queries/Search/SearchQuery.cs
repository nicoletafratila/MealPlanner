using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<UnitModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
