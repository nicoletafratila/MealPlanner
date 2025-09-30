using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<ProductModel>>
    {
        public string? CategoryId { get; set; }
        public QueryParameters<ProductModel>? QueryParameters { get; set; }
    }
}
