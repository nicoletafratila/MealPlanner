using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Commands.Update
{
    /// <summary>
    /// Command to update an application user.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        public ApplicationUserEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(ApplicationUserEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}