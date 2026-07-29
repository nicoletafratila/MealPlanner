using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Duende.IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Features.Authentication
{
    public static class JwtTokenFactory
    {
        public static string GenerateJwtToken(IList<Claim> claims)
        {
            var expiration = DateTimeOffset.UtcNow.AddHours(1);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Common.Constants.MealPlanner.Issuer,
                audience: Common.Constants.MealPlanner.ApiScope,
                claims: claims,
                expires: expiration.UtcDateTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static IList<Claim> GetClaims(Data.Entities.ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, user.UserName!),
                new(JwtClaimTypes.Scope, Common.Constants.MealPlanner.ApiScope),
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            return claims;
        }
    }
}
