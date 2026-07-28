using System.Security.Cryptography;
using System.Text;
using Identity.Data.Entities;

namespace Identity.Api.Features.Authentication
{
    public static class RefreshTokenGenerator
    {
        public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(30);

        public static string Generate() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public static string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        public static (RefreshToken Entity, string RawToken) CreateEntity(string userId, TimeSpan lifetime)
        {
            var rawToken = Generate();
            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = Hash(rawToken),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.Add(lifetime)
            };
            return (entity, rawToken);
        }
    }
}
