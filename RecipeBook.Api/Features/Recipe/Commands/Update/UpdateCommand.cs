using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.Update
{
    /// <summary>
    /// Command to update a recipe.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The recipe edit model to update. Must be non-null for a valid command.
        /// </summary>
        public RecipeEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(RecipeEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}