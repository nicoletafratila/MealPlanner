using Common.Models;
using Identity.Api.Features.Authentication.Resources;
using Identity.Api.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler(
        UserManager<Common.Data.Entities.ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), AuthenticationMessages.ModelCannotBeNull);

            try
            {
                var user = await userManager.FindByEmailAsync(request.Model.EmailAddress);
                if (user is not null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    try
                    {
                        await emailService.SendPasswordResetAsync(user.Email!, user.Id, token, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                    }
                }

                // Always return success to avoid revealing whether an email address is registered
                return CommandResponse.Success(AuthenticationMessages.ForgotPasswordEmailSent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during forgot password for '{Email}'.", request.Model.EmailAddress);
                return CommandResponse.Failed(AuthenticationMessages.ForgotPasswordError);
            }
        }
    }
}
