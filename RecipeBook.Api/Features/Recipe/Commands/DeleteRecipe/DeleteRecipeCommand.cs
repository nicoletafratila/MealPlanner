using MediatR;

namespace RecipeBook.Api.Features.Recipe.Commands.DeleteRecipe
{
    public class DeleteRecipeCommand : IRequest<DeleteRecipeCommandResponse>
    {
        public int Id { get; set; }
    }
}
