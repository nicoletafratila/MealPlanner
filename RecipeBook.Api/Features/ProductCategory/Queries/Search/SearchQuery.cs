using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    /// <summary>
    /// Query to search product categories with pagination and filtering.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<ProductCategoryModel>>
    {
        /// <summary>
        /// Pagination, filtering, and sorting parameters for product categories.
        /// </summary>
        public QueryParameters<ProductCategoryModel>? QueryParameters { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(QueryParameters<ProductCategoryModel> queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}