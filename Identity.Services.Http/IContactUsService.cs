using Common.Models;
using Identity.Shared.Models;

namespace Identity.Services.Http
{
    public interface IContactUsService
    {
        Task<CommandResponse?> SendAsync(ContactUsModel model, CancellationToken cancellationToken = default);
    }
}
