using Common.Models;
using Identity.Api.Features.Authentication.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler(
        UserManager<Data.Entities.ApplicationUser> userManager,
        ILogger<ConfirmEmailCommandHandler> logger) : IRequestHandler<ConfirmEmailCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var user = await userManager.FindByIdAsync(request.UserId);
                if (user is null)
                    return CommandResponse.Failed(AuthenticationMessages.EmailConfirmationInvalid);

                var result = await userManager.ConfirmEmailAsync(user, request.Token);
                if (!result.Succeeded)
                {
                    var isConcurrencyConflict = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.ConcurrencyFailure));
                    if (isConcurrencyConflict && await IsAlreadyConfirmedAsync(request.UserId))
                    {
                        logger.LogDebug("Email confirmed by a concurrent request for user {UserId}", request.UserId);
                        return CommandResponse.Success(AuthenticationMessages.EmailConfirmationSuccess);
                    }

                    logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", request.UserId,
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                    return CommandResponse.Failed(AuthenticationMessages.EmailConfirmationFailed);
                }

                user.IsActive = true;
                var activationResult = await userManager.UpdateAsync(user);
                if (!activationResult.Succeeded)
                {
                    logger.LogWarning("Failed to activate user {UserId} after email confirmation: {Errors}", request.UserId,
                        string.Join("; ", activationResult.Errors.Select(e => e.Description)));
                }

                logger.LogDebug("Email confirmed for user {UserId}", request.UserId);
                return CommandResponse.Success(AuthenticationMessages.EmailConfirmationSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during email confirmation for user '{UserId}'.", request.UserId);
                return CommandResponse.Failed(AuthenticationMessages.RegistrationError);
            }
        }

        private async Task<bool> IsAlreadyConfirmedAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            return user?.EmailConfirmed == true;
        }
    }
}
