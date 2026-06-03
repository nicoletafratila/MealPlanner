using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Delete
{
    /// <summary>
    /// Command to delete a product category by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the product category to delete.
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