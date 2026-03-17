using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Command to log in a user.
    /// </summary>
    public class LoginCommand : IRequest<CommandResponse?>
    {
        public LoginModel? Model { get; set; }

        public LoginCommand()
        {
        }

        public LoginCommand(LoginModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}