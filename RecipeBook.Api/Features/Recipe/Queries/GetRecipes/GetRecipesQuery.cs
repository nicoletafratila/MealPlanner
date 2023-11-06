using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetRecipes
{
    public class GetRecipesQuery : IRequest<IList<RecipeModel>>
    {
    }
}
