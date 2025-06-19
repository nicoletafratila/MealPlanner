using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IAuthenticationService
    {
        Task<string?> LoginAsync(LoginModel model);
        Task<string?> RegisterAsync(RegistrationModel model);
    }
}
