using System.Net.Http.Json;
using Common.Http;
using Common.Models;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Services.Http
{
    public class ContactUsService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ContactUsService> logger)
        : ServiceBase(httpClient, tokenProvider), IContactUsService
    {
        private readonly string _controller = IdentityControllers.ContactUsUrl;

        public async Task<CommandResponse?> SendAsync(ContactUsModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                // Contact form does not require authentication
                using var response = await HttpClient.PostAsJsonAsync(
                    $"{_controller}/{IdentityControllers.SendRoute}", model, JsonOptions, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("ContactUsService.SendAsync failed with status {StatusCode}.", response.StatusCode);
                    return CommandResponse.Failed(Resources.ContactUsServiceMessages.SendFailed);
                }
                return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken)
                    ?? CommandResponse.Failed(Resources.ContactUsServiceMessages.SendFailed);
            }
            catch (HttpRequestException ex) { logger.LogError(ex, "HTTP error during SendAsync."); return CommandResponse.Failed(Resources.ContactUsServiceMessages.NetworkError); }
            catch (System.Text.Json.JsonException ex) { logger.LogError(ex, "Deserialization error during SendAsync."); return CommandResponse.Failed(Resources.ContactUsServiceMessages.InvalidResponse); }
        }
    }
}
