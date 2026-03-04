using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Update
{
    /// <summary>
    /// Command to update a single recipe category.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The recipe category edit model to update. Must be non-null for a valid command.
        /// </summary>
        public RecipeCategoryEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(RecipeCategoryEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}