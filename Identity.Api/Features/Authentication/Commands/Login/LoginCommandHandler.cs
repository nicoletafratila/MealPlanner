using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Constants;
using Common.Data.Entities;
using Duende.IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        private readonly ILogger<LoginCommandHandler> _logger = logger;

        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userManager.FindByNameAsync(request.Model!.Username!);
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(request.Model!.Username!, request.Model?.Password!, true, false);
                    await signInManager.SignInAsync(user, true);

                    if (result.Succeeded)
                    {
                        return new LoginCommandResponse { Success = result.Succeeded, Message = string.Empty };
                    }
                    return new LoginCommandResponse { Success = result.Succeeded, Message = string.Empty };
                }
                return new LoginCommandResponse { Success = false, Message = "User was not found." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new LoginCommandResponse { Message = "An error occurred when authenticating the user." };
            }
        }

        private string CreateJWT(ApplicationUser user, string role)
        {
            var secretkey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(MealPlannerKey.SigningKey));
            var credentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtClaimTypes.Id, user.Id),
                new Claim(JwtClaimTypes.Name, user.UserName),
                new Claim(JwtClaimTypes.GivenName, user.FirstName),
                new Claim(JwtClaimTypes.FamilyName, user.LastName),
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.Role, role),
                new Claim(JwtClaimTypes.WebSite, "http://mealplanner.com")
            };

            var token = new JwtSecurityToken(issuer: "mealplanner.com", audience: "mealplanner.com", claims: claims, expires: DateTime.Now.AddMinutes(60), signingCredentials: credentials);
            var jsth = new JwtSecurityTokenHandler();
            return jsth.WriteToken(token);
        }
    }
}


