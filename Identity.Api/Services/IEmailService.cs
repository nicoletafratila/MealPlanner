namespace Identity.Api.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default);
    }
}
