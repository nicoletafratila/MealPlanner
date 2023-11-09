using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.UpdateRecipe
{
    public class UpdateRecipeCommand : IRequest<UpdateRecipeCommandResponse>
    {
        public EditRecipeModel? Model { get; set; }
    }
}
