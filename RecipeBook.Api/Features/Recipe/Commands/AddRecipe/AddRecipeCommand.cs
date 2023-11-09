using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.AddRecipe
{
    public class AddRecipeCommand : IRequest<AddRecipeCommandResponse>
    {
        public EditRecipeModel? Model { get; set; }
    }
}
