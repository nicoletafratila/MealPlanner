using System.Security.Claims;
using System.Text.Json;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace MealPlanner.UI.Web.Services.Identities
{
    public class CustomAuthenticationState : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorage;
        private static AuthenticationState AnonymousState => new(new ClaimsPrincipal(new ClaimsIdentity()));

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public CustomAuthenticationState(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _sessionStorage.GetItemAsync<string>(Common.Constants.MealPlanner.AuthToken);

                if (string.IsNullOrWhiteSpace(token) || IsTokenExpired(token))
                    return AnonymousState;

                if (!TryReadPayload(token, out var payload))
                    return AnonymousState;

                var claims = CreateClaimsFromPayload(payload);
                var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return AnonymousState;
            }
        }

        private static bool TryReadPayload(string jwt, out Dictionary<string, JsonElement> payload)
        {
            payload = [];

            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return false;

            try
            {
                var jsonBytes = ParseBase64WithoutPadding(parts[1]);
                var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes, JsonOptions);
                if (result is null)
                    return false;

                payload = result;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static IEnumerable<Claim> CreateClaimsFromPayload(Dictionary<string, JsonElement> payload)
        {
            var claims = new List<Claim>();

            foreach (var (key, value) in payload)
            {
                if (string.Equals(key, "exp", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "nbf", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "iat", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null && item.ValueKind != JsonValueKind.Undefined)
                            claims.Add(new Claim(key, item.ToString()));
                    }
                }
                else if (value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined)
                {
                    claims.Add(new Claim(key, value.ToString()));
                }
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }

        private static bool IsTokenExpired(string token)
        {
            if (!TryReadPayload(token, out var payload))
                return true;

            if (!payload.TryGetValue("exp", out var expElement))
                return true;

            if (expElement.ValueKind != JsonValueKind.Number ||
                !expElement.TryGetInt64(out var expSeconds))
            {
                return true;
            }

            var expTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            return expTime <= DateTimeOffset.UtcNow;
        }
    }
}
