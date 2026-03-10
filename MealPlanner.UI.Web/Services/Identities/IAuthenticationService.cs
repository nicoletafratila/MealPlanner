using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identities
{
    public interface IAuthenticationService
    {
        Task<CommandResponse?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> LogoutAsync(CancellationToken cancellationToken = default);
    }
}