using System.Net.Http.Json;
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
                using var response = await httpClient.PostAsJsonAsync(
                    $"{_controller}/{IdentityControllers.LoginRoute}", model, JsonOptions, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("LoginAsync failed with status {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.AuthenticationFailed : error);
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginCommandResponse>(JsonOptions, cancellationToken);
                if (loginResponse is null) return CommandResponse.Failed(Resources.AuthenticationServiceMessages.AuthenticationFailed);
                if (loginResponse.Succeeded && !string.IsNullOrWhiteSpace(loginResponse.JwtBearer))
                {
                    await TokenProvider.SetTokenAsync(loginResponse.JwtBearer, cancellationToken);
                    return loginResponse;
                }
                return CommandResponse.Failed(loginResponse.Message ?? Resources.AuthenticationServiceMessages.AuthenticationFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during LoginAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorAuthentication); }
            catch (TaskCanceledException ex) { logger.LogError(ex, "Timeout during LoginAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorAuthentication); }
            catch (System.Text.Json.JsonException ex) { logger.LogError(ex, "Deserialization error during LoginAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.InvalidResponseAuthentication); }
        }

        public async Task<CommandResponse?> LogoutAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsync($"{_controller}/{IdentityControllers.LogoutRoute}", content: null, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("LogoutAsync failed with status {StatusCode}.", response.StatusCode);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.LogoutFailed : error);
                }
                var result = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
                if (result?.Succeeded == true) await TokenProvider.RemoveTokenAsync(cancellationToken);
                return result ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.LogoutFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during LogoutAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorLogout); }
        }

        public async Task<CommandResponse?> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync($"{_controller}/{IdentityControllers.RegisterRoute}", model, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("RegisterAsync failed with status {StatusCode}.", response.StatusCode);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.RegistrationFailed : error);
                }
                return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken)
                    ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.RegistrationFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during RegisterAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorRegistration); }
        }

        public async Task<CommandResponse?> ForgotPasswordAsync(ForgotPasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync($"{_controller}/{IdentityControllers.ForgotPasswordRoute}", model, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode) { logger.LogWarning("ForgotPasswordAsync failed."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed); }
                return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken) ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during ForgotPasswordAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed); }
        }

        public async Task<CommandResponse?> ResetPasswordAsync(ResetPasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync($"{_controller}/{IdentityControllers.ResetPasswordRoute}", model, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode) { logger.LogWarning("ResetPasswordAsync failed."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed); }
                return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken) ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during ResetPasswordAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed); }
        }

        public async Task<CommandResponse?> ChangePasswordAsync(ChangePasswordModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                await EnsureAuthAsync(cancellationToken);
                using var response = await httpClient.PostAsJsonAsync($"{_controller}/{IdentityControllers.ChangePasswordRoute}", model, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.ChangePasswordFailed : error);
                }
                return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken) ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ChangePasswordFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during ChangePasswordAsync."); return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ChangePasswordFailed); }
        }
    }
}
