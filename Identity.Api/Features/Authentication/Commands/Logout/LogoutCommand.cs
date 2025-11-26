using Common.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.Logout
{
    public class LogoutCommand : IRequest<CommandResponse?>
    {
    }
}
