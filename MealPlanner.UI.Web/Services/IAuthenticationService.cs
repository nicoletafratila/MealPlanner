using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IAuthenticationService
    {
        Task<LoginCommandResponse> LoginAsync(LoginModel model);
        Task<CommandResponse> RegisterAsync(RegistrationModel model);
    }
}
