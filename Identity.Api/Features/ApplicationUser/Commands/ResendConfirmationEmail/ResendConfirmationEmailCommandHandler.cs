using Common.Models;
using Identity.Api.Features.ApplicationUser.Resources;
using Identity.Api.Features.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Commands.ResendConfirmationEmail
{
    public class ResendConfirmationEmailCommandHandler(
        UserManager<Data.Entities.ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ResendConfirmationEmailCommandHandler> logger) : IRequestHandler<ResendConfirmationEmailCommand, CommandResponse?>
    {
        private readonly UserManager<Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.UserId))
                return CommandResponse.Failed(ApplicationUserMessages.UserIdRequired);

            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user is null)
                    return CommandResponse.Failed(string.Format(ApplicationUserMessages.UserNotFoundWithId, request.UserId));

                if (user.EmailConfirmed)
                    return CommandResponse.Failed(ApplicationUserMessages.EmailAlreadyConfirmed);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailConfirmationAsync(user.Email!, user.Id, token, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resending the confirmation email for user {UserId}", request.UserId);
                return CommandResponse.Failed(ApplicationUserMessages.ResendConfirmationEmailError);
            }
        }
    }
}
