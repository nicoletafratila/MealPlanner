using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.SearchByCategories
{
    public class SearchByCategoriesQuery : IRequest<IList<RecipeCategoryModel>>
    {
        public string? CategoryIds { get; set; }
    }
}
