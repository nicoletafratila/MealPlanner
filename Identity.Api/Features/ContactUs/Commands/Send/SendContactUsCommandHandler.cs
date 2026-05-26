using Common.Models;
using Identity.Api.Features.ContactUs.Resources;
using Identity.Api.Features.Email;
using MediatR;

namespace Identity.Api.Features.ContactUs.Commands.Send
{
    public class SendContactUsCommandHandler(
        IEmailService emailService,
        ILogger<SendContactUsCommandHandler> logger) : IRequestHandler<SendContactUsCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(SendContactUsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), ContactUsMessages.ModelRequired);

            try
            {
                await emailService.SendContactUsAsync(
                    request.Model.Name,
                    request.Model.EmailAddress,
                    request.Model.Subject,
                    request.Model.Message,
                    cancellationToken);

                return CommandResponse.Success(ContactUsMessages.SendSuccessful);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send contact us email from '{Email}'.", request.Model.EmailAddress);
                return CommandResponse.Failed(ContactUsMessages.SendFailed);
            }
        }
    }
}
