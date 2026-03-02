using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    /// <summary>
    /// Command to delete a unit by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the unit to delete.
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