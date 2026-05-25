using Common.Models;
using Identity.Api.Features.ApplicationUser.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Commands.Unlock
{
    public class UnlockCommandHandler(
        UserManager<Common.Data.Entities.ApplicationUser> userManager,
        ILogger<UnlockCommandHandler> logger) : IRequestHandler<UnlockCommand, CommandResponse?>
    {
        private readonly UserManager<Common.Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ILogger<UnlockCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(UnlockCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.UserId))
                return CommandResponse.Failed(ApplicationUserMessages.UserIdRequired);

            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user is null)
                    return CommandResponse.Failed(string.Format(ApplicationUserMessages.UserNotFoundWithId, request.UserId));

                await _userManager.ResetAccessFailedCountAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, null);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while unlocking user {UserId}", request.UserId);
                return CommandResponse.Failed(ApplicationUserMessages.UnlockUserError);
            }
        }
    }
}
