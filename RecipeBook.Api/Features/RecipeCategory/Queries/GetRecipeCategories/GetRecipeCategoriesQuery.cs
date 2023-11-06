using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetRecipeCategories
{
    public class GetRecipeCategoriesQuery : IRequest<IList<RecipeCategoryModel>>
    {
    }
}
