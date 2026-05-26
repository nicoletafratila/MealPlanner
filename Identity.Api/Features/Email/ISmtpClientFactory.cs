namespace Identity.Api.Features.Email
{
    public interface ISmtpClientFactory
    {
        ISmtpClient Create(string host, int port);
    }
}
