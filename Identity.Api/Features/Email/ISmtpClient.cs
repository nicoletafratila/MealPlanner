using System.Net;
using System.Net.Mail;

namespace Identity.Api.Features.Email
{
    public interface ISmtpClient : IDisposable
    {
        bool EnableSsl { get; set; }
        ICredentialsByHost? Credentials { get; set; }
        Task SendMailAsync(MailMessage message, CancellationToken cancellationToken);
    }
}
