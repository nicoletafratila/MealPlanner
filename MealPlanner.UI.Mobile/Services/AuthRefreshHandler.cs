using System.Net;
using System.Net.Http.Headers;
using Common.Http;
using Identity.Services.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public class AuthRefreshHandler(IServiceProvider services, ITokenProvider tokenProvider) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            var authService = services.GetRequiredService<AuthenticationService>();
            var refreshed = await authService.RefreshAsync(cancellationToken);
            var token = refreshed ? await tokenProvider.GetTokenAsync(cancellationToken) : null;
            if (string.IsNullOrWhiteSpace(token))
                return response;

            response.Dispose();

            using var retryRequest = await CloneRequestAsync(request);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            if (request.Content is not null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(bytes);
                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var header in request.Headers)
            {
                if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
