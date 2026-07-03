namespace Identity.Api.Features.Email
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default);
        Task SendPasswordResetAsync(string toEmail, string userId, string token, Common.Constants.InputSource? source = null, CancellationToken cancellationToken = default);
        Task SendContactUsAsync(string fromName, string fromEmail, string subject, string message, CancellationToken cancellationToken = default);
    }
}
