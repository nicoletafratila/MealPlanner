using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Add
{
    /// <summary>
    /// Command to add a new shopping list.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The shopping list edit model to add. Must be non-null for a valid command.
        /// </summary>
        public ShoppingListEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(ShoppingListEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}