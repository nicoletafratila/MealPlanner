using Common.Models;
using Identity.Api.Features.Authentication.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(
        UserManager<Common.Data.Entities.ApplicationUser> userManager,
        ILogger<ResetPasswordCommandHandler> logger) : IRequestHandler<ResetPasswordCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), AuthenticationMessages.ModelCannotBeNull);

            try
            {
                var user = await userManager.FindByIdAsync(request.Model.UserId);
                if (user is null)
                    return CommandResponse.Failed(AuthenticationMessages.PasswordResetFailed);

                var result = await userManager.ResetPasswordAsync(user, request.Model.Token, request.Model.NewPassword);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Password reset failed for user {UserId}: {Errors}", request.Model.UserId,
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                    return CommandResponse.Failed(AuthenticationMessages.PasswordResetFailed);
                }

                logger.LogDebug("Password reset successfully for user {UserId}", request.Model.UserId);
                return CommandResponse.Success(AuthenticationMessages.PasswordResetSuccessful);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during password reset for user '{UserId}'.", request.Model.UserId);
                return CommandResponse.Failed(AuthenticationMessages.PasswordResetError);
            }
        }
    }
}
