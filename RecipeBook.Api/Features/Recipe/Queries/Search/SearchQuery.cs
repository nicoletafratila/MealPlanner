using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<RecipeModel>>
    {
        public string? CategoryId { get; set; }
        public QueryParameters? QueryParameters { get; set; }
    }
}
