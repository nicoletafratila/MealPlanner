using System.Net.Http.Json;using Common.Models; using Common.Pagination; using Identity.Shared.Models; using Microsoft.Extensions.Logging;

namespace Identity.Services.Core
{
    public interface IApplicationUserService
    {
        Task<PagedList<ApplicationUserModel>?> SearchAsync(QueryParameters<ApplicationUserModel>? queryParameters = null, CancellationToken cancellationToken = default);
        Task<ApplicationUserEditModel?> GetEditAsync(string name, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UnlockAsync(string userId, CancellationToken cancellationToken = default);
    }
}
