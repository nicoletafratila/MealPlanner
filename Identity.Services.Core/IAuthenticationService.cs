using System.Net.Http.Json;using Common.Models; using Identity.Shared.Models; using Microsoft.Extensions.Logging;

namespace Identity.Services.Core
{
    public interface IAuthenticationService
    {
        Task<CommandResponse?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> LogoutAsync(CancellationToken cancellationToken = default);
        Task<CommandResponse?> ForgotPasswordAsync(ForgotPasswordModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default);
    }
}
