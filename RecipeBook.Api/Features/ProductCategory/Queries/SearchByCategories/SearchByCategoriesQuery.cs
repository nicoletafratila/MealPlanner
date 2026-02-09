using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories
{
    public class SearchByCategoriesQuery : IRequest<IList<ProductCategoryModel>>
    {
        public string? CategoryIds { get; set; }
    }
}
