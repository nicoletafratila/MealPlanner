using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Common.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public class MobileAuthStateService(ITokenProvider tokenProvider)
    {
        public event Action? AuthStateChanged;

        public async Task<ClaimsPrincipal> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var token = await tokenProvider.GetTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                var payload = ParsePayload(token);
                if (payload is null)
                    return new ClaimsPrincipal(new ClaimsIdentity());

                if (TryGetExpiration(payload, out var exp) && exp < DateTimeOffset.UtcNow)
                    return new ClaimsPrincipal(new ClaimsIdentity());

                var claims = BuildClaims(payload);
                return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            }
            catch
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
        }

        public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync(cancellationToken);
            return user.Identity?.IsAuthenticated == true;
        }

        public async Task<string?> GetUserNameAsync(CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync(cancellationToken);
            return user.Identity?.Name
                ?? user.FindFirst("name")?.Value
                ?? user.FindFirst("sub")?.Value;
        }

        public void NotifyAuthStateChanged() => AuthStateChanged?.Invoke();

        private static Dictionary<string, JsonElement>? ParsePayload(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        }

        private static bool TryGetExpiration(Dictionary<string, JsonElement> payload, out DateTimeOffset expiration)
        {
            expiration = default;
            if (!payload.TryGetValue("exp", out var expElement))
                return false;

            if (expElement.ValueKind == JsonValueKind.Number && expElement.TryGetInt64(out var exp))
            {
                expiration = DateTimeOffset.FromUnixTimeSeconds(exp);
                return true;
            }

            if (long.TryParse(expElement.GetString(), out exp))
            {
                expiration = DateTimeOffset.FromUnixTimeSeconds(exp);
                return true;
            }

            return false;
        }

        private static IEnumerable<Claim> BuildClaims(Dictionary<string, JsonElement> payload)
        {
            var skip = new HashSet<string> { "exp", "nbf", "iat" };
            var claims = new List<Claim>();
            foreach (var (key, value) in payload)
            {
                if (skip.Contains(key)) continue;
                if (value.ValueKind == JsonValueKind.Array)
                    claims.AddRange(value.EnumerateArray().Select(e => new Claim(key, e.GetString() ?? string.Empty)));
                else
                    claims.Add(new Claim(key, value.GetString() ?? value.ToString()));
            }
            return claims;
        }
    }
}
