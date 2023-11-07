using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.SearchProducts
{
    public class SearchProductsQuery : IRequest<PagedList<ProductModel>>
    {
        public string? CategoryId { get; set; }
        public QueryParameters? QueryParameters { get; set; }
    }
}
