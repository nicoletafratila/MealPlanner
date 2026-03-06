using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Update
{
    /// <summary>
    /// Command to update a product category.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The product category edit model to update. Must be non-null for a valid command.
        /// </summary>
        public ProductCategoryEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(ProductCategoryEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}