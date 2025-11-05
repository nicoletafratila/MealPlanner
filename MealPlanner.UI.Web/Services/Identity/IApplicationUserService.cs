using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identity
{
    public interface IApplicationUserService
    {
        Task<ApplicationUserEditModel?> GetEditAsync(string name);
    }
}
