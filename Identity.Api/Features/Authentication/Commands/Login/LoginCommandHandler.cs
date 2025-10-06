using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Data.Entities;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userManager.FindByNameAsync(request!.Model!.Username!);
                if (user == null)
                    return CommandResponse.Failed("Invalid credentials");

                var roles = await userManager.GetRolesAsync(user);

                var result = await signInManager.PasswordSignInAsync(request!.Model!.Username!, request!.Model!.Password!, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var claims = GetClaims(user, roles);
                    var token = GenerateJwtToken(claims);
                    return new LoginCommandResponse
                    {
                        Message = "Login successful.",
                        Succeeded = true,
                        JwtBearer = token,
                        Claims = claims.Select(x => new KeyValuePair<string, string>(x.Type, x.Value)).ToList(),
                    };
                }

                if (result.IsLockedOut)
                    return CommandResponse.Failed("User is locked out");

                return CommandResponse.Failed("User/password not found.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when authenticating the user.");
            }
        }

        private string GenerateJwtToken(IList<Claim> claims)
        {
            var expiration = DateTimeOffset.UtcNow.AddHours(1);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "MealPlanner",
                audience: "MealPlanner",
                claims: claims,
                expires: expiration.UtcDateTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private IList<Claim> GetClaims(ApplicationUser user, IList<string> roles)
        {
            return new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user!.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user!.UserName !),
                new Claim(ClaimTypes.Role, string.Join(",", roles))
            };
        }
    }
}