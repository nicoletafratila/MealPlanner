using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetRecipe
{
    public class GetRecipeQuery : IRequest<RecipeModel>
    {
        public int Id { get; set; }
    }
}
