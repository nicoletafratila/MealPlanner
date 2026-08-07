using System.Text;
using System.Text.Json;

namespace Common.Http.Tests
{
    [TestFixture]
    public class JwtUserIdExtractorTests
    {
        [Test]
        public void GetUserId_ValidToken_ReturnsSubClaim()
        {
            var token = CreateToken(new Dictionary<string, object> { ["sub"] = "user-123" });

            Assert.That(JwtUserIdExtractor.GetUserId(token), Is.EqualTo("user-123"));
        }

        [Test]
        public void GetUserId_DifferentSubClaims_ReturnDifferentValues()
        {
            var tokenA = CreateToken(new Dictionary<string, object> { ["sub"] = "user-1" });
            var tokenB = CreateToken(new Dictionary<string, object> { ["sub"] = "user-2" });

            Assert.That(JwtUserIdExtractor.GetUserId(tokenA), Is.Not.EqualTo(JwtUserIdExtractor.GetUserId(tokenB)));
        }

        [Test]
        public void GetUserId_MissingSubClaim_ReturnsNull()
        {
            var token = CreateToken(new Dictionary<string, object> { ["name"] = "someone" });

            Assert.That(JwtUserIdExtractor.GetUserId(token), Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not-a-jwt")]
        [TestCase("only.two")]
        public void GetUserId_InvalidToken_ReturnsNull(string? token)
        {
            Assert.That(JwtUserIdExtractor.GetUserId(token), Is.Null);
        }

        private static string CreateToken(Dictionary<string, object> claims)
        {
            var header = Base64UrlEncode("""{"alg":"none","typ":"JWT"}"""u8.ToArray());
            var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(claims)));
            return $"{header}.{payload}.signature";
        }

        private static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
