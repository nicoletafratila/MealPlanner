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

                var result = await signInManager.PasswordSignInAsync(request!.Model!.Username!, request!.Model!.Password!, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var token = GenerateJwtToken(user);
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

        private string GenerateJwtToken(ApplicationUser user)
        {
            var expiration = DateTimeOffset.UtcNow.AddHours(1);
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user!.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user !.UserName !)
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

//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Common.Data.Entities;
//using Common.Models;
//using Identity.Shared.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;

//[ApiController]
//[Route("api/[controller]")]
//public class AuthenticationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : ControllerBase
//{
//    [HttpPost("login")]
//    public async Task<IActionResult> Login([FromBody] LoginModel model)
//    {
//        var user = await userManager.FindByNameAsync(model.Username);
//        if (user == null)
//            return Unauthorized(new { message = "Invalid credentials" });

//        var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

//        if (result.Succeeded)
//        {
//            var token = GenerateJwtToken(user);
//            Response.Cookies.Append(
//            Common.Constants.MealPlanner.AuthCookie,
//            token,
//            new CookieOptions
//            {
//                HttpOnly = true,          // Prevents JS access (mitigates XSS risk)
//                Secure = true,            // Only sent on HTTPS
//                SameSite = SameSiteMode.Strict // Restricts cross-site sending
//            });
//            return Ok(new LoginCommandResponse
//            {
//                Succeeded=true,
//                JwtBearer = token,
//                Username = user.UserName
//            });
//        }

//        if (result.IsLockedOut)
//            return BadRequest(new { message = "User is locked out" });

//        return Unauthorized(new { message = "Invalid login attempt" });
//    }

//    private string GenerateJwtToken(ApplicationUser user)
//    {
//        var expiration = DateTimeOffset.UtcNow.AddHours(1);
//        var claims = new[]
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//            new Claim(ClaimTypes.NameIdentifier, user.Id),
//            new Claim(ClaimTypes.Name, user.UserName)
//        };

//        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey));
//        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//        var token = new JwtSecurityToken(
//            issuer: "MealPlanner",
//            audience: "MealPlanner",
//            claims: claims,
//            expires: expiration.UtcDateTime,
//            signingCredentials: creds);

//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }
//}