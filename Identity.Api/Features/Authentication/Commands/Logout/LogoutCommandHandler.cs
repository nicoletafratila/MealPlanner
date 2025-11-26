using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Logout
{
    public class LogoutCommandHandler(SignInManager<Common.Data.Entities.ApplicationUser> signInManager) : IRequestHandler<LogoutCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await signInManager.SignOutAsync();
            return CommandResponse.Success();
        }
    }
}
