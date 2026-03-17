using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Logout
{
    /// <summary>
    /// Handles logging out the current user.
    /// </summary>
    public class LogoutCommandHandler(SignInManager<Common.Data.Entities.ApplicationUser> signInManager) : IRequestHandler<LogoutCommand, CommandResponse?>
    {
        private readonly SignInManager<Common.Data.Entities.ApplicationUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));

        public async Task<CommandResponse?> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            await _signInManager.SignOutAsync();
            return CommandResponse.Success();
        }
    }
}