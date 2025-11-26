using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<CommandResponse?>
    {
        public LoginModel? Model { get; set; }
    }
}
