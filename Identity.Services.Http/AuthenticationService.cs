using Common.Http;
using Common.Models;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;

namespace Identity.Services.Http
{
    public class AuthenticationService(HttpClient httpClient, ITokenProvider tokenProvider, IAuthStateNotifier authStateNotifier)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            IdentityControllers.AuthenticationUrl
            ?? throw new ArgumentException("Authentication controller URL is not configured.");

        public async Task<(bool Success, string? Error)> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync<LoginModel, LoginCommandResponse>($"{_controller}/login", model, cancellationToken);
                if (response?.Succeeded == true && !string.IsNullOrWhiteSpace(response.JwtBearer))
                {
                    await TokenProvider.SetTokenAsync(response.JwtBearer, cancellationToken);
                    authStateNotifier.NotifyAuthStateChanged();
                    return (true, null);
                }
                return (false, response?.Message ?? "Login failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync($"{_controller}/register", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Registration failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            try { await PostAsync<object>($"{_controller}/logout", new { }, cancellationToken); } catch { }
            await TokenProvider.RemoveTokenAsync(cancellationToken);
            authStateNotifier.NotifyAuthStateChanged();
        }

        public async Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync($"{_controller}/forgot-password", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync($"{_controller}/reset-password", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync($"{_controller}/change-password", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
