using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    /// <summary>
    /// Command to add a new unit.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The unit model to add. Must be non-null for a valid command.
        /// </summary>
        public UnitEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(UnitEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}