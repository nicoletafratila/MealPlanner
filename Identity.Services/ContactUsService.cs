using System.Net.Http.Json;
using System.Text.Json;
using Common.Constants;
using Common.Models;
using Identity.Api;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Services
{
    public class ContactUsService(
        HttpClient httpClient,
        IdentityApiConfig identityApiConfig,
        ILogger<ContactUsService> logger) : IContactUsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _contactUsController =
            identityApiConfig.Controllers![IdentityControllers.ContactUs]
            ?? throw new ArgumentException("ContactUs controller URL is not configured.", nameof(identityApiConfig));

        public async Task<CommandResponse?> SendAsync(
            ContactUsModel model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    $"{_contactUsController}/send",
                    model,
                    JsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("ContactUsService.SendAsync failed with status {StatusCode}. Body: {Body}", response.StatusCode, error);
                    return CommandResponse.Failed(Resources.ContactUsServiceMessages.SendFailed);
                }

                var result = await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
                return result ?? CommandResponse.Failed(Resources.ContactUsServiceMessages.SendFailed);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error during ContactUsService.SendAsync.");
                return CommandResponse.Failed(Resources.ContactUsServiceMessages.NetworkError);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse during ContactUsService.SendAsync.");
                return CommandResponse.Failed(Resources.ContactUsServiceMessages.InvalidResponse);
            }
        }
    }
}
