using System.Net;
using System.Net.Mail;
using Identity.Api.Features.Authentication.Resources;

namespace Identity.Api.Services
{
    public class SmtpEmailService(IConfiguration configuration, IWebHostEnvironment environment, ISmtpClientFactory smtpClientFactory, ILogger<SmtpEmailService> logger) : IEmailService
    {
        public async Task SendEmailConfirmationAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default)
        {
            var emailSettings = configuration.GetSection("Email");
            var baseUrl = configuration["IdentityApi:BaseUrl"] ?? "https://localhost:5001";

            var encodedToken = Uri.EscapeDataString(token);
            var confirmUrl = $"{baseUrl}/api/authentication/confirm-email?userId={userId}&token={encodedToken}";

            var templatePath = Path.Combine(environment.ContentRootPath, "EmailTemplates", "EmailConfirmation.html");
            var body = (await File.ReadAllTextAsync(templatePath, cancellationToken))
                .Replace("{{ConfirmUrl}}", confirmUrl)
                .Replace("{{EmailConfirmation_Title}}", AuthenticationMessages.EmailConfirmation_Title)
                .Replace("{{EmailConfirmation_Heading}}", AuthenticationMessages.EmailConfirmation_Heading)
                .Replace("{{EmailConfirmation_Body}}", AuthenticationMessages.EmailConfirmation_Body)
                .Replace("{{EmailConfirmation_ButtonText}}", AuthenticationMessages.EmailConfirmation_ButtonText)
                .Replace("{{EmailConfirmation_FallbackText}}", AuthenticationMessages.EmailConfirmation_FallbackText)
                .Replace("{{EmailConfirmation_FooterText}}", AuthenticationMessages.EmailConfirmation_FooterText);

            using var message = new MailMessage
            {
                From = new MailAddress(emailSettings["From"]!),
                Subject = AuthenticationMessages.EmailConfirmation_Subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = smtpClientFactory.Create(emailSettings["Host"]!, int.Parse(emailSettings["Port"]!));
            client.EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
            client.Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]);

            await client.SendMailAsync(message, cancellationToken);
            logger.LogDebug("Confirmation email sent to {Email}", toEmail);
        }
    }
}
