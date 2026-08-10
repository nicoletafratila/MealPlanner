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

        /// <summary>
        /// When true, skips loading the full image for each result and returns only the thumbnail —
        /// used by reference-data lookups that fetch many rows at once for pickers.
        /// </summary>
        public bool ThumbnailOnly { get; set; }

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