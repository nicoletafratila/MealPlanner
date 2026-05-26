using Common.Models;
using Identity.Shared.Models;

namespace Identity.Services
{
    public interface IContactUsService
    {
        Task<CommandResponse?> SendAsync(ContactUsModel model, CancellationToken cancellationToken = default);
    }
}
