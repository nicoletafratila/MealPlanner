using Common.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.Logout
{
    /// <summary>
    /// Command to log out the current user.
    /// </summary>
    public class LogoutCommand : IRequest<CommandResponse?>
    {
    }
}