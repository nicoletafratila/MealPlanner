using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Common.Data.Entities;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var signInResult = await signInManager.PasswordSignInAsync(request!.Model!.Username!, request!.Model!.Password!, isPersistent: false, lockoutOnFailure: false);
                if (signInResult.Succeeded)
                {
                    var user = await userManager.FindByNameAsync(request!.Model!.Username!);
                    return new LoginCommandResponse
                    {
                        Message = "Login successful.",
                        Username = user!.UserName,
                        JwtBearer = await CreateJWTAsync(user),
                        Succeeded = true
                    };
                }

                return new LoginCommandResponse
                {
                    Message = "User/password not found.",
                    Succeeded = false
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return new LoginCommandResponse
                {
                    Message = "An error occurred when authenticating the user.",
                    Succeeded = false
                };
            }
        }

        private async Task<string> CreateJWTAsync(ApplicationUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: "domain.com", audience: "domain.com", claims: claims, expires: DateTime.Now.AddMinutes(60), signingCredentials: credentials); // NOTE: ENTER DOMAIN HERE
            var jsth = new JwtSecurityTokenHandler();
            return jsth.WriteToken(token);
        }
    }
}
