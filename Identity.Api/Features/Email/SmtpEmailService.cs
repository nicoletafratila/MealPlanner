using System.Net;
using System.Net.Mail;
using Identity.Api.Features.Authentication.Resources;
using Identity.Api.Features.ContactUs.Resources;

namespace Identity.Api.Features.Email
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

        public async Task SendPasswordResetAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default)
        {
            var emailSettings = configuration.GetSection("Email");
            var baseUrl = configuration["IdentityApi:BaseUrl"] ?? "https://localhost:5001";

            var encodedToken = Uri.EscapeDataString(token);
            var resetUrl = $"{baseUrl}/api/authentication/reset-password-redirect?userId={userId}&token={encodedToken}";

            var templatePath = Path.Combine(environment.ContentRootPath, "EmailTemplates", "PasswordReset.html");
            var body = (await File.ReadAllTextAsync(templatePath, cancellationToken))
                .Replace("{{ResetUrl}}", resetUrl)
                .Replace("{{PasswordReset_Title}}", AuthenticationMessages.PasswordReset_Title)
                .Replace("{{PasswordReset_Heading}}", AuthenticationMessages.PasswordReset_Heading)
                .Replace("{{PasswordReset_Body}}", AuthenticationMessages.PasswordReset_Body)
                .Replace("{{PasswordReset_ButtonText}}", AuthenticationMessages.PasswordReset_ButtonText)
                .Replace("{{PasswordReset_FallbackText}}", AuthenticationMessages.PasswordReset_FallbackText)
                .Replace("{{PasswordReset_FooterText}}", AuthenticationMessages.PasswordReset_FooterText);

            using var message = new MailMessage
            {
                From = new MailAddress(emailSettings["From"]!),
                Subject = AuthenticationMessages.PasswordReset_Subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = smtpClientFactory.Create(emailSettings["Host"]!, int.Parse(emailSettings["Port"]!));
            client.EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
            client.Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]);

            await client.SendMailAsync(message, cancellationToken);
            logger.LogDebug("Password reset email sent to {Email}", toEmail);
        }

        public async Task SendContactUsAsync(string fromName, string fromEmail, string subject, string message, CancellationToken cancellationToken = default)
        {
            var emailSettings = configuration.GetSection("Email");

            var body = $"<p><strong>{ContactUsMessages.EmailFromLabel}:</strong> {fromName} &lt;{fromEmail}&gt;</p>" +
                       $"<p><strong>{ContactUsMessages.EmailSubjectLabel}:</strong> {subject}</p>" +
                       $"<hr/><p><strong>{ContactUsMessages.EmailMessageLabel}:</strong></p><p>{message.Replace("\n", "<br/>")}</p>";

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["From"]!),
                Subject = $"{ContactUsMessages.EmailSubjectPrefix}: {subject}",
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(emailSettings["From"]!);
            mailMessage.ReplyToList.Add(new MailAddress(fromEmail, fromName));

            using var client = smtpClientFactory.Create(emailSettings["Host"]!, int.Parse(emailSettings["Port"]!));
            client.EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
            client.Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]);

            await client.SendMailAsync(mailMessage, cancellationToken);
            logger.LogDebug("Contact us email sent from {Email}", fromEmail);
        }
    }
}
