using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Delete
{
    /// <summary>
    /// Command to delete a shopping list by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the shopping list to delete.
        /// </summary>
        public int Id { get; set; }

        public DeleteCommand()
        {
        }

        public DeleteCommand(int id)
        {
            Id = id;
        }
    }
}