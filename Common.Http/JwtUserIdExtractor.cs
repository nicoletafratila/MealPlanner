using System.Text.Json;

namespace Common.Http
{
    public static class JwtUserIdExtractor
    {
        public static string? GetUserId(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            try
            {
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - (payload.Length % 4));
                var json = Convert.FromBase64String(padded);
                var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                return claims is not null && claims.TryGetValue("sub", out var sub) ? sub.GetString() : null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
