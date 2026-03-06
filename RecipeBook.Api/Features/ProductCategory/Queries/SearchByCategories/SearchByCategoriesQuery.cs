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

        public SearchByCategoriesQuery()
        {
        }

        public SearchByCategoriesQuery(string? categoryIds)
        {
            CategoryIds = categoryIds;
        }
    }
}