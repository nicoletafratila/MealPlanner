using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    /// <summary>
    /// Command to add a new shop.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The shop edit model to add. Must be non-null for a valid command.
        /// </summary>
        public ShopEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(ShopEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}