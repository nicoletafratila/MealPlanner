using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Commands.Update
{
    /// <summary>
    /// Command to update a unit.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The unit model to update. Must be non-null for a valid command.
        /// </summary>
        public UnitEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(UnitEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}