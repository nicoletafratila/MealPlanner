using Common.Models;
using Common.Pagination;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identities
{
    public interface IApplicationUserService
    {
        Task<PagedList<ApplicationUserModel>?> SearchAsync(QueryParameters<ApplicationUserModel>? queryParameters = null, CancellationToken cancellationToken = default);
        Task<ApplicationUserEditModel?> GetEditAsync(string name, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default);
    }
}