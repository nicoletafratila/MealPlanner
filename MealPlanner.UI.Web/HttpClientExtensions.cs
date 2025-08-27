using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MealPlanner.UI.Web
{
    public static class HttpClientExtensions
    {
        public static async Task EnsureAuthorizationHeaderAsync(this HttpClient httpClient, TokenProvider tokenProvider)
        {
            var token = await tokenProvider.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token) &&
                (httpClient.DefaultRequestHeaders.Authorization == null ||
                 httpClient.DefaultRequestHeaders.Authorization.Parameter != token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            }
        }
    }
}
