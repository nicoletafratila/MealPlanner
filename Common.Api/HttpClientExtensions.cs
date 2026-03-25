using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Common.Api
{
    public static class HttpClientExtensions
    {
        public static async Task EnsureAuthorizationHeaderAsync(
            this HttpClient httpClient,
            TokenProvider tokenProvider,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(tokenProvider);

            cancellationToken.ThrowIfCancellationRequested();

            var token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
            httpClient.EnsureAuthorizationHeader(token);
        }

        public static void EnsureAuthorizationHeader(this HttpClient httpClient, string? token)
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            if (!string.IsNullOrWhiteSpace(token) &&
                (httpClient.DefaultRequestHeaders.Authorization == null ||
                 httpClient.DefaultRequestHeaders.Authorization.Parameter != token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            }
        }

        public static string GetCleanToken(string? authHeader)
        {
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith(
                    JwtBearerDefaults.AuthenticationScheme + " ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring(JwtBearerDefaults.AuthenticationScheme.Length).Trim();
            }

            return string.Empty;
        }
    }
}