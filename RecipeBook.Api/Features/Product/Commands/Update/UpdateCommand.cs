using System;
using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.Update
{
    /// <summary>
    /// Command to update a product.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The product edit model to update. Must be non-null for a valid command.
        /// </summary>
        public ProductEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(ProductEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}