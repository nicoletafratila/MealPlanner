using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.Add
{
    /// <summary>
    /// Command to add a new product.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The product edit model to add. Must be non-null for a valid command.
        /// </summary>
        public ProductEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(ProductEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}