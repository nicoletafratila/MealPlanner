using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Identity.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace MealPlanner.UI.Web.Services.Identities
{
    public class ApplicationUserService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        IdentityApiConfig identityApiConfig,
        ILogger<ApplicationUserService> logger) : IApplicationUserService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _userController =
            identityApiConfig.Controllers![IdentityControllers.ApplicationUser]
            ?? throw new ArgumentException("ApplicationUser controller URL is not configured.", nameof(identityApiConfig));

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        public async Task<ApplicationUserEditModel?> GetEditAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Username must not be empty.", nameof(name));

            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString(
                $"{_userController}/edit",
                new Dictionary<string, string?>
                {
                    ["username"] = name
                });

            try
            {
                return await httpClient.GetFromJsonAsync<ApplicationUserEditModel?>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize ApplicationUserEditModel for username {UserName}", name);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            await EnsureAuthAsync();

            using var response = await httpClient.PutAsJsonAsync(_userController, model, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogWarning("ApplicationUser UpdateAsync failed with status code {StatusCode}. Body: {Body}", response.StatusCode, error);

                return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? "Update user failed." : error);
            }

            try
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for ApplicationUser UpdateAsync. Model {@Model}", model);
                return CommandResponse.Failed("Invalid response from user update endpoint.");
            }
        }
    }
}