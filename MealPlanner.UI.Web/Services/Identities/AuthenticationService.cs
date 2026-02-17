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

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        public async Task<CommandResponse?> LoginAsync(LoginModel model)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync($"{_authController}/login", model, JsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("LoginAsync failed with status code {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? "Authentication failed." : error);
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginCommandResponse>(JsonOptions);
                if (loginResponse is null)
                {
                    logger.LogError("LoginAsync: response body could not be deserialized into LoginCommandResponse.");
                    return CommandResponse.Failed("Authentication failed.");
                }

                if (loginResponse.Succeeded && !string.IsNullOrWhiteSpace(loginResponse.JwtBearer))
                {
                    await tokenProvider.SetTokenAsync(loginResponse.JwtBearer);
                    return loginResponse;
                }

                logger.LogWarning("LoginAsync did not succeed. Message: {Message}", loginResponse.Message);
                return CommandResponse.Failed(loginResponse.Message ?? "Authentication failed.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during LoginAsync.");
                return CommandResponse.Failed("Network error during authentication.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize LoginCommandResponse during LoginAsync.");
                return CommandResponse.Failed("Invalid response from authentication server.");
            }
        }

        public async Task<CommandResponse?> LogoutAsync()
        {
            try
            {
                await EnsureAuthAsync();

                using var response = await httpClient.PostAsync($"{_authController}/logout", content: null);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("LogoutAsync failed with status code {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? "Logout failed." : error);
                }

                var logoutResponse = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions);
                if (logoutResponse is null)
                {
                    logger.LogError("LogoutAsync: response body could not be deserialized into CommandResponse.");
                    return CommandResponse.Failed("Logout failed.");
                }

                if (logoutResponse.Succeeded)
                {
                    await tokenProvider.RemoveTokenAsync();
                    return logoutResponse;
                }

                logger.LogWarning("LogoutAsync did not succeed. Message: {Message}", logoutResponse.Message);
                return CommandResponse.Failed(logoutResponse.Message ?? "Logout failed.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during LogoutAsync.");
                return CommandResponse.Failed("Network error during logout.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during LogoutAsync.");
                return CommandResponse.Failed("Invalid response from authentication server.");
            }
        }

        public async Task<CommandResponse?> RegisterAsync(RegistrationModel model)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync($"{_authController}/register", model, JsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("RegisterAsync failed with status code {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? "Registration failed." : error);
                }

                var registerResponse = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions);
                if (registerResponse is null)
                {
                    logger.LogError("RegisterAsync: response body could not be deserialized into CommandResponse.");
                    return CommandResponse.Failed("Registration failed.");
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
                return CommandResponse.Failed("Network error during registration.");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during RegisterAsync.");
                return CommandResponse.Failed("Invalid response from registration server.");
            }
        }
    }
}