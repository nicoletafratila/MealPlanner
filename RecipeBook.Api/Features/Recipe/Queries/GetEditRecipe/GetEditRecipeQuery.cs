using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEditRecipe
{
    public class GetEditRecipeQuery : IRequest<EditRecipeModel>
    {
        public int Id { get; set; }
    }
}
