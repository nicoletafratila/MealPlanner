using Common.Models;
using Common.Pagination;
using Identity.Shared.Models;

namespace Identity.Services.Http
{
    public interface IApplicationUserService
    {
        Task<PagedList<ApplicationUserModel>?> SearchAsync(QueryParameters<ApplicationUserModel>? queryParameters = null, CancellationToken cancellationToken = default);
        Task<ApplicationUserEditModel?> GetEditAsync(string name, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UnlockAsync(string userId, CancellationToken cancellationToken = default);
    }
}
