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