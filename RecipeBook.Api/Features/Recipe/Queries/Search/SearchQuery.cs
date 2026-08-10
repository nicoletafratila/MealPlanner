using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.Search
{
    /// <summary>
    /// Query to search recipes, optionally filtered by category, with pagination and filtering.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<RecipeModel>>
    {
        /// <summary>
        /// Optional recipe category id filter (as string).
        /// </summary>
        public string? CategoryId { get; set; }

        /// <summary>
        /// Pagination, filtering, and sorting parameters for recipes.
        /// </summary>
        public QueryParameters<RecipeModel>? QueryParameters { get; set; }

        /// <summary>
        /// When true, skips loading the full image for each result and returns only the thumbnail —
        /// used by reference-data lookups that fetch many rows at once for pickers.
        /// </summary>
        public bool ThumbnailOnly { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(string? categoryId, QueryParameters<RecipeModel>? queryParameters)
        {
            CategoryId = categoryId;
            QueryParameters = queryParameters;
        }
    }
}