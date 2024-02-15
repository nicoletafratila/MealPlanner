using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchProductCategory
{
    public class SearchProductCategoryQuery : IRequest<PagedList<ProductCategoryModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
