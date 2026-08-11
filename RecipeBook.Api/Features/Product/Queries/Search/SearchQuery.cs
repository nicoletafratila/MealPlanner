using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    /// <summary>
    /// Query to search products, optionally filtered by category, with pagination and filtering.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<ProductModel>>
    {
        /// <summary>
        /// Optional product category id filter (as string).
        /// </summary>
        public string? CategoryId { get; set; }

        /// <summary>
        /// Pagination, filtering, and sorting parameters for products.
        /// </summary>
        public QueryParameters<ProductModel>? QueryParameters { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(string? categoryId, QueryParameters<ProductModel>? queryParameters)
        {
            CategoryId = categoryId;
            QueryParameters = queryParameters;
        }
    }
}