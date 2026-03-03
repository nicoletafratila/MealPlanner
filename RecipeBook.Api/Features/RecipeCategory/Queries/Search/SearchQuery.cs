using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.Search
{
    /// <summary>
    /// Query to search recipe categories with pagination and filtering.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<RecipeCategoryModel>>
    {
        /// <summary>
        /// Pagination, filtering, and sorting parameters for recipe categories.
        /// </summary>
        public QueryParameters<RecipeCategoryModel>? QueryParameters { get; set; }

        public SearchQuery()
        {
        }

        public SearchQuery(QueryParameters<RecipeCategoryModel> queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }
}