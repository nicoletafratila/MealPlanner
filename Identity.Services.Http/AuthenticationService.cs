using System.Net.Http.Json;
using System.Text.Json;
using Common.Http;
using Common.Models;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Services.Http
{
    public class AuthenticationService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<AuthenticationService> logger)
        : ServiceBase(httpClient, tokenProvider), IAuthenticationService
    {
        private readonly string _controller = IdentityControllers.AuthenticationUrl;

        public async Task<CommandResponse?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var loginResponse = await PostAsync<LoginModel, LoginCommandResponse>(
                    $"{_controller}/{IdentityControllers.LoginRoute}", model, cancellationToken);
                if (loginResponse is null) return CommandResponse.Failed(Resources.AuthenticationServiceMessages.AuthenticationFailed);
                if (loginResponse.Succeeded && !string.IsNullOrWhiteSpace(loginResponse.JwtBearer))
                {
                    await TokenProvider.SetTokenAsync(loginResponse.JwtBearer, cancellationToken);
                    return loginResponse;
                }
                return CommandResponse.Failed(loginResponse.Message ?? Resources.AuthenticationServiceMessages.AuthenticationFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during LoginAsync."); return CommandResponse.Failed(ex.Message ?? Resources.AuthenticationServiceMessages.AuthenticationFailed); }
            catch (TaskCanceledException ex) { logger.LogError(ex, "Timeout during LoginAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorAuthentication); }
            catch (JsonException ex) { logger.LogError(ex, "Deserialization error during LoginAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.InvalidResponseAuthentication); }
        }

        public async Task<CommandResponse?> LogoutAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await HttpClient.PostAsync($"{_controller}/{IdentityControllers.LogoutRoute}", content: null, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await ReadErrorMessageAsync(response, cancellationToken);
                    logger.LogWarning("LogoutAsync failed with status {StatusCode}.", response.StatusCode);
                    return CommandResponse.Failed(error ?? Resources.AuthenticationServiceMessages.LogoutFailed);
                }
                var result = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
                if (result?.Succeeded == true) await TokenProvider.RemoveTokenAsync(cancellationToken);
                return result ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.LogoutFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during LogoutAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorLogout); }
        }

        public async Task<CommandResponse?> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync($"{_controller}/{IdentityControllers.RegisterRoute}", model, cancellationToken); }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during RegisterAsync."); return CommandResponse.Failed(ex.Message ?? Resources.AuthenticationServiceMessages.RegistrationFailed); }
        }

        public async Task<CommandResponse?> ForgotPasswordAsync(ForgotPasswordModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync($"{_controller}/{IdentityControllers.ForgotPasswordRoute}", model, cancellationToken) ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed); }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during ForgotPasswordAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed); }
        }

        public async Task<CommandResponse?> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync($"{_controller}/{IdentityControllers.ResetPasswordRoute}", model, cancellationToken) ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed); }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during ResetPasswordAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed); }
        }

        public async Task<CommandResponse?> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync($"{_controller}/{IdentityControllers.ChangePasswordRoute}", model, cancellationToken) ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ChangePasswordFailed); }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during ChangePasswordAsync."); return CommandResponse.Failed(ex.Message ?? Resources.AuthenticationServiceMessages.ChangePasswordFailed); }
        }
    }
}
