using Common.Http;
using Common.Models;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Services.Http
{
    public class IdentityService(
        HttpClient httpClient,
        ITokenProvider tokenProvider,
        IAuthStateNotifier authStateNotifier,
        ILogger<IdentityService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            IdentityControllers.AuthenticationUrl
            ?? throw new ArgumentException("Authentication controller URL is not configured.");

        public async Task<(bool Success, string? Error)> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync<LoginModel, LoginCommandResponse>(
                    $"{_controller}/{IdentityControllers.LoginRoute}", model, cancellationToken);
                if (response?.Succeeded == true && !string.IsNullOrWhiteSpace(response.JwtBearer))
                {
                    await TokenProvider.SetTokenAsync(response.JwtBearer, cancellationToken);
                    authStateNotifier.NotifyAuthStateChanged();
                    return (true, null);
                }
                return (false, response?.Message ?? "Login failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} LoginAsync failed for user {Username}", nameof(IdentityService), model.Username);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(
                    $"{_controller}/{IdentityControllers.RegisterRoute}", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Registration failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} RegisterAsync failed for user {Username}", nameof(IdentityService), model.Username);
                return (false, ex.Message);
            }
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            try { await PostAsync<object>($"{_controller}/{IdentityControllers.LogoutRoute}", new { }, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "{ServiceName} LogoutAsync failed", nameof(IdentityService)); }
            await TokenProvider.RemoveTokenAsync(cancellationToken);
            authStateNotifier.NotifyAuthStateChanged();
        }

        public async Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(
                    $"{_controller}/{IdentityControllers.ForgotPasswordRoute}", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} ForgotPasswordAsync failed for user {Email}", nameof(IdentityService), model.EmailAddress);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(
                    $"{_controller}/{IdentityControllers.ResetPasswordRoute}", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} ResetPasswordAsync failed", nameof(IdentityService));
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(
                    $"{_controller}/{IdentityControllers.ChangePasswordRoute}", model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} ChangePasswordAsync failed", nameof(IdentityService));
                return (false, ex.Message);
            }
        }
    }
}
