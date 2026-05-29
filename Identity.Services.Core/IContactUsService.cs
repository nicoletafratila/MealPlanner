using System.Net.Http.Json;using Common.Models; using Identity.Shared.Models; using Microsoft.Extensions.Logging;

namespace Identity.Services.Core
{
    public interface IContactUsService
    {
        Task<CommandResponse?> SendAsync(ContactUsModel model, CancellationToken cancellationToken = default);
    }
}
