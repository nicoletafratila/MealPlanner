using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Update
{
    /// <summary>
    /// Command to update a shop.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The shop edit model to update. Must be non-null for a valid command.
        /// </summary>
        public ShopEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(ShopEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}