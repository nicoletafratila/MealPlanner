using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    /// <summary>
    /// Command to add a new product category.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The product category edit model to add. Must be non-null for a valid command.
        /// </summary>
        public ProductCategoryEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(ProductCategoryEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}