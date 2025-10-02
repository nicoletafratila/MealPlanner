using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Common.Api
{
    public static class HttpClientExtensions
    {
        public static async Task EnsureAuthorizationHeaderAsync(this HttpClient httpClient, TokenProvider tokenProvider)
        {
            var token = await tokenProvider.GetTokenAsync();
            httpClient.EnsureAuthorizationHeader(token);
        }

        public static void EnsureAuthorizationHeader(this HttpClient httpClient, string? token)
        {
            if (!string.IsNullOrWhiteSpace(token) &&
                (httpClient.DefaultRequestHeaders.Authorization == null ||
                 httpClient.DefaultRequestHeaders.Authorization.Parameter != token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            }
        }

        public static string CleanToken(string? authHeader)
        {
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith(JwtBearerDefaults.AuthenticationScheme + " ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring(JwtBearerDefaults.AuthenticationScheme.Length).Trim();
            }
            return string.Empty;
        }
    }
}
