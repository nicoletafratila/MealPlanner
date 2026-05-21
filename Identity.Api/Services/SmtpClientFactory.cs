namespace Identity.Api.Services
{
    public sealed class SmtpClientFactory : ISmtpClientFactory
    {
        public ISmtpClient Create(string host, int port) => new SmtpClientWrapper(host, port);
    }
}
