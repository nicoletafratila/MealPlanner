using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginModel model);
        Task<CommandResponse> RegisterAsync(RegistrationModel model);
    }
}
