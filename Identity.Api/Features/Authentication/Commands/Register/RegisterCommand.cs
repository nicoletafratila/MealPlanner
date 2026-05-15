using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.Register
{
    public class RegisterCommand : IRequest<CommandResponse?>
    {
        public RegisterCommand() { }

        public RegisterCommand(RegistrationModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public RegistrationModel Model { get; init; } = default!;
    }
}
