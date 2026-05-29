using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Common.Http;

namespace MealPlanner.UI.Mobile.Services
{
    public class MobileAuthStateService(ITokenProvider tokenProvider) : IAuthStateNotifier
    {
        public event Action? AuthStateChanged;

        public async Task<ClaimsPrincipal> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var token = await tokenProvider.GetTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                var claims = ParseClaims(token).ToList();
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim is not null && long.TryParse(expClaim.Value, out var exp))
                {
                    if (DateTimeOffset.FromUnixTimeSeconds(exp) < DateTimeOffset.UtcNow)
                        return new ClaimsPrincipal(new ClaimsIdentity());
                }
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

        private static IEnumerable<Claim> ParseClaims(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return [];

            var payload = parts[1];
            var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict is null) return [];

            var skip = new HashSet<string> { "exp", "nbf", "iat" };
            var claims = new List<Claim>();
            foreach (var (key, value) in dict)
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
