using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.Add
{
    /// <summary>
    /// Command to add a new recipe.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Recipe edit model to add. Must be non-null for a valid command.
        /// </summary>
        public RecipeEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(RecipeEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}