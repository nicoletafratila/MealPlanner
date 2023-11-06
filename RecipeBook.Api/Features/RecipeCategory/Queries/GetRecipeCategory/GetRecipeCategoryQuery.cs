using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetRecipeCategory
{
    public class GetRecipeCategoryQuery : IRequest<IList<RecipeCategoryModel>>
    {
    }
}
