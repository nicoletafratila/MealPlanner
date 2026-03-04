using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    /// <summary>
    /// Command to add a new recipe category.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The recipe category edit model to add. Must be non-null for a valid command.
        /// </summary>
        public RecipeCategoryEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(RecipeCategoryEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}