using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http.Headers;

namespace MealPlanner.UI.Web.Services
{
    public class JwtAuthorizationMessageHandler(ILocalStorageService localStorage) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await localStorage.GetItemAsStringAsync(Common.Constants.MealPlanner.TokenKey);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    if (token.StartsWith("\"") && token.EndsWith("\""))
                    {
                        token = token.Substring(1, token.Length - 2);
                    }

                    request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
                }
            }
            catch (Exception ex) { }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
