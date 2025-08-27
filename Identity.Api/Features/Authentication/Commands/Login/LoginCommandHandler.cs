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
                    var token = GenerateJwtToken(user, roles);
                    //Response.Cookies.Append(
                    //Common.Constants.MealPlanner.AuthCookie,
                    //token,
                    //new CookieOptions
                    //{
                    //    HttpOnly = true,          // Prevents JS access (mitigates XSS risk)
                    //    Secure = true,            // Only sent on HTTPS
                    //    SameSite = SameSiteMode.Strict // Restricts cross-site sending
                    //});
                    return new LoginCommandResponse
                    {
                        Message = "Login successful.",
                        Succeeded = true,
                        JwtBearer = token,
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

        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync(); // Removes/invalidate the cookie
        //    return Ok(new { message = "Logout successful." });
        //}

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var expiration = DateTimeOffset.UtcNow.AddHours(1);
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user!.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user!.UserName !),
                    new Claim(ClaimTypes.Role, string.Join(",", roles)),
                };

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
    }
}