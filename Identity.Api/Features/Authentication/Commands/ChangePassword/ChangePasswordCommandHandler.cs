using Common.Models;
using Identity.Api.Features.Authentication.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler(
        UserManager<Identity.Data.Entities.ApplicationUser> userManager,
        ILogger<ChangePasswordCommandHandler> logger) : IRequestHandler<ChangePasswordCommand, CommandResponse?>
    {
        private readonly UserManager<Identity.Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ILogger<ChangePasswordCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), AuthenticationMessages.ModelCannotBeNull);

            try
            {
                var user = await _userManager.FindByIdAsync(request.Model.UserId);
                if (user is null)
                    return CommandResponse.Failed(AuthenticationMessages.ChangePasswordFailed);

                var result = await _userManager.ChangePasswordAsync(user, request.Model.CurrentPassword, request.Model.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Password change failed for user {UserId}: {Errors}", request.Model.UserId, errors);
                    return CommandResponse.Failed(errors);
                }

                _logger.LogDebug("Password changed successfully for user {UserId}", request.Model.UserId);
                return CommandResponse.Success(AuthenticationMessages.ChangePasswordSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user '{UserId}'.", request.Model.UserId);
                return CommandResponse.Failed(AuthenticationMessages.ChangePasswordError);
            }
        }
    }
}
