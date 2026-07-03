using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    /// <summary>
    /// Command to delete a recipe category by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the recipe category to delete.
        /// </summary>
        public Guid Id { get; set; }

        public DeleteCommand()
        {
        }

        public DeleteCommand(Guid id)
        {
            Id = id;
        }
    }
}