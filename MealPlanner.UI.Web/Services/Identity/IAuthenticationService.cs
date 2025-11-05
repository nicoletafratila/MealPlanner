using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identity
{
    public interface IAuthenticationService
    {
        Task<CommandResponse> LoginAsync(LoginModel model);
        Task<CommandResponse> RegisterAsync(RegistrationModel model);
        Task<CommandResponse> LogoutAsync();
    }
}
