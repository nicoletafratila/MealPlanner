using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories
{
    /// <summary>
    /// Query to retrieve product categories by a list of ids.
    /// </summary>
    public class SearchByCategoriesQuery : IRequest<IList<ProductCategoryModel>>
    {
        /// <summary>
        /// Comma-separated list of category ids, e.g. "1,2,3".
        /// </summary>
        public string? CategoryIds { get; set; }

        /// <summary>
        /// Optional filters to apply, keeping the result consistent with the caller's current filter state.
        /// </summary>
        public IEnumerable<FilterItem>? Filters { get; set; }

        /// <summary>
        /// Optional sorting to apply, keeping the result consistent with the caller's current sort state.
        /// </summary>
        public IEnumerable<SortingModel>? Sorting { get; set; }

        public SearchByCategoriesQuery()
        {
        }

        public SearchByCategoriesQuery(string? categoryIds)
        {
            CategoryIds = categoryIds;
        }
    }
}