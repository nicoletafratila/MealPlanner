using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identities
{
    public interface IApplicationUserService
    {
        Task<ApplicationUserEditModel?> GetEditAsync(string name);
        Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model);
    }
}
