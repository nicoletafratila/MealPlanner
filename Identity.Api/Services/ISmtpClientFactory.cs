namespace Identity.Api.Services
{
    public interface ISmtpClientFactory
    {
        ISmtpClient Create(string host, int port);
    }
}
