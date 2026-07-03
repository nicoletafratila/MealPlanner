using System.Net.Mail;
using System.Net;

namespace Identity.Api.Features.Email
{
    public class SmtpClientWrapper(string host, int port) : ISmtpClient
    {
        private readonly SmtpClient _client = new(host, port);

        public bool EnableSsl
        {
            get => _client.EnableSsl;
            set => _client.EnableSsl = value;
        }

        public ICredentialsByHost? Credentials
        {
            get => _client.Credentials;
            set => _client.Credentials = value;
        }

        public Task SendMailAsync(MailMessage message, CancellationToken cancellationToken)
            => _client.SendMailAsync(message, cancellationToken);

        public void Dispose() => _client.Dispose();
    }
}
