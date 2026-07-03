namespace Identity.Api.Features.Email
{
    public class SmtpClientFactory : ISmtpClientFactory
    {
        public ISmtpClient Create(string host, int port) => new SmtpClientWrapper(host, port);
    }
}
