using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<ProductCategoryModel>>
    {
        public QueryParameters<ProductCategoryModel>? QueryParameters { get; set; }
    }
}
