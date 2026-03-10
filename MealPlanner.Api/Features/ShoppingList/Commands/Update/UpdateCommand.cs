using System;
using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Update
{
    /// <summary>
    /// Command to update a shopping list.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The shopping list edit model to update. Must be non-null for a valid command.
        /// </summary>
        public ShoppingListEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(ShoppingListEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}