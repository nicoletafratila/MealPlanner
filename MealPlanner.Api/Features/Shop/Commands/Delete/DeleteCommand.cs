using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Delete
{
    /// <summary>
    /// Command to delete a shop by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the shop to delete.
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