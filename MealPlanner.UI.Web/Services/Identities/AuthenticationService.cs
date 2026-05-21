using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identities
{
    public class AuthenticationService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        IdentityApiConfig identityApiConfig,
        ILogger<AuthenticationService> logger) : IAuthenticationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _authController =
            identityApiConfig.Controllers![IdentityControllers.Authentication]
            ?? throw new ArgumentException("Authentication controller URL is not configured.", nameof(identityApiConfig));

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        public async Task<CommandResponse?> LoginAsync(
            LoginModel model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    $"{_authController}/login",
                    model,
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning(
                        "LoginAsync failed with status code {StatusCode}. Body: {Body}",
                        response.StatusCode,
                        error);
                    return CommandResponse.Failed(
                        string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.AuthenticationFailed : error);
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginCommandResponse>(
                    JsonOptions,
                    cancellationToken);

                if (loginResponse is null)
                {
                    logger.LogError("LoginAsync: response body could not be deserialized into LoginCommandResponse.");
                    return CommandResponse.Failed(Resources.AuthenticationServiceMessages.AuthenticationFailed);
                }

                if (loginResponse.Succeeded && !string.IsNullOrWhiteSpace(loginResponse.JwtBearer))
                {
                    await tokenProvider.SetTokenAsync(loginResponse.JwtBearer, cancellationToken);
                    return loginResponse;
                }

                logger.LogWarning("LoginAsync did not succeed. Message: {Message}", loginResponse.Message);
                return CommandResponse.Failed(loginResponse.Message ?? Resources.AuthenticationServiceMessages.AuthenticationFailed);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during LoginAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorAuthentication);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize LoginCommandResponse during LoginAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.InvalidResponseAuthentication);
            }
        }

        public async Task<CommandResponse?> LogoutAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                await EnsureAuthAsync(cancellationToken);

                using var response = await httpClient.PostAsync(
                    $"{_authController}/logout",
                    content: null,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning(
                        "LogoutAsync failed with status code {StatusCode}. Body: {Body}",
                        response.StatusCode,
                        error);
                    return CommandResponse.Failed(
                        string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.LogoutFailed : error);
                }

                var logoutResponse = await response.Content.ReadFromJsonAsync<CommandResponse>(
                    JsonOptions,
                    cancellationToken);

                if (logoutResponse is null)
                {
                    logger.LogError("LogoutAsync: response body could not be deserialized into CommandResponse.");
                    return CommandResponse.Failed(Resources.AuthenticationServiceMessages.LogoutFailed);
                }

                if (logoutResponse.Succeeded)
                {
                    await tokenProvider.RemoveTokenAsync(cancellationToken);
                    return logoutResponse;
                }

                logger.LogWarning("LogoutAsync did not succeed. Message: {Message}", logoutResponse.Message);
                return CommandResponse.Failed(logoutResponse.Message ?? Resources.AuthenticationServiceMessages.LogoutFailed);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during LogoutAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorLogout);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during LogoutAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.InvalidResponseAuthentication);
            }
        }

        public async Task<CommandResponse?> RegisterAsync(
            RegistrationModel model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    $"{_authController}/register",
                    model,
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning(
                        "RegisterAsync failed with status code {StatusCode}. Body: {Body}",
                        response.StatusCode,
                        error);
                    return CommandResponse.Failed(
                        string.IsNullOrWhiteSpace(error) ? Resources.AuthenticationServiceMessages.RegistrationFailed : error);
                }

                var registerResponse = await response.Content.ReadFromJsonAsync<CommandResponse>(
                    JsonOptions,
                    cancellationToken);

                if (registerResponse is null)
                {
                    logger.LogError("RegisterAsync: response body could not be deserialized into CommandResponse.");
                    return CommandResponse.Failed(Resources.AuthenticationServiceMessages.RegistrationFailed);
                }

                if (!registerResponse.Succeeded)
                {
                    logger.LogWarning("RegisterAsync did not succeed. Message: {Message}", registerResponse.Message);
                }

                return registerResponse;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during RegisterAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.NetworkErrorRegistration);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during RegisterAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.InvalidResponseRegistration);
            }
        }

        public async Task<CommandResponse?> ForgotPasswordAsync(
            ForgotPasswordModel model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    $"{_authController}/forgot-password",
                    model,
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("ForgotPasswordAsync failed with status {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed);
                }

                var result = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
                return result ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during ForgotPasswordAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during ForgotPasswordAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ForgotPasswordFailed);
            }
        }

        public async Task<CommandResponse?> ResetPasswordAsync(
            ResetPasswordModel model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    $"{_authController}/reset-password",
                    model,
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("ResetPasswordAsync failed with status {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed);
                }

                var result = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
                return result ?? CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during ResetPasswordAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during ResetPasswordAsync.");
                return CommandResponse.Failed(Resources.AuthenticationServiceMessages.ResetPasswordFailed);
            }
        }
    }
}