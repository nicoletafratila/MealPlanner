using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Product.Commands.Delete
{
    /// <summary>
    /// Command to delete a product by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the product to delete.
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