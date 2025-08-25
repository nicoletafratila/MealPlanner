//using Common.Models;
//using Identity.Api.Features.Authentication.Commands.Login;
//using Identity.Shared.Models;
//using MediatR;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Mvc;

//namespace Identity.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [EnableCors]
//    public class AuthenticationController(ISender mediator) : ControllerBase
//    {
//        [HttpPost("login")]
//        public async Task<CommandResponse> LoginAsync(LoginModel model)
//        {
//            LoginCommand command = new()
//            {
//                Model = model
//            };
//            return await mediator.Send(command);
//        }

//        //[HttpPost("register")]
//        //public async Task<LoginCommandResponse> RegisterAsync(LoginModel model)
//        //{
//        //    LoginCommand command = new()
//        //    {
//        //        Model = model
//        //    };
//        //    return await _mediator.Send(command);
//        //}
//    }
//}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Data.Entities;
using Common.Models;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var token = GenerateJwtToken(user);
            Response.Cookies.Append(
            Common.Constants.MealPlanner.AuthCookie,
            token,
            new CookieOptions
            {
                HttpOnly = true,          // Prevents JS access (mitigates XSS risk)
                Secure = true,            // Only sent on HTTPS
                SameSite = SameSiteMode.Strict // Restricts cross-site sending
            });
            return Ok(new LoginResponse
            {
                Succeeded=true,
                Token = token,
                Username = user.UserName
            });
        }

        if (result.IsLockedOut)
            return BadRequest(new { message = "User is locked out" });

        return Unauthorized(new { message = "Invalid login attempt" });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var expiration = DateTimeOffset.UtcNow.AddHours(1);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
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


//[ApiController]
//[Route("api/[controller]")]
//public class AuthenticationController : Controller
//{
//    private readonly SignInManager<ApplicationUser> _signInManager;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly ILogger<AuthenticationController> _logger;

//    public AuthenticationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger)
//    {
//        _signInManager = signInManager;
//        _userManager = userManager;
//        _logger = logger;
//    }

//    [HttpPost("login")]
//    public async Task<IActionResult> Login(LoginModel model)
//    {
//        // 1. Authenticate
//        var signInResult = await _signInManager.PasswordSignInAsync(
//            model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

//        if (signInResult.Succeeded)
//        {
//            // 2. (Optionally) Reload page/signal re-auth via frontend if using Blazor Server

//            // 3. Diagnostic: Confirm Set-Cookie will be issued
//            if (!HttpContext.Response.Headers.ContainsKey("Set-Cookie"))
//                _logger.LogInformation("Expecting Set-Cookie header; Identity should issue automatically.");

//            //Response.Cookies.Append("Set-Cookie", HttpContext.Response.Headers["Set-Cookie"], new CookieOptions
//            //{
//            //    Secure = true,
//            //    SameSite = SameSiteMode.Strict 
//            //});
//            // 4. Return a response for the Blazor client—your UI
//            return Ok(new { message = "Login successful.", username = model.Username, succeeded = true });
//        }
//        return Unauthorized(new { message = "User/password not found." });
//    }

//    [HttpPost("logout")]
//    public async Task<IActionResult> Logout()
//    {
//        await _signInManager.SignOutAsync();
//        return Ok(new { message = "Logout successful." });
//    }

//    //[HttpPost("diagnostic-cookie")]
//    //public async Task<IActionResult> DiagnosticCookie()
//    //{
//    //    await HttpContext.SignInAsync(
//    //        CookieAuthenticationDefaults.AuthenticationScheme,
//    //        new ClaimsPrincipal(new ClaimsIdentity(
//    //            new[] { new Claim(ClaimTypes.Name, "debuguser") },
//    //            CookieAuthenticationDefaults.AuthenticationScheme
//    //        ))
//    //    );
//    //    return Ok(new { message = "Manual cookie issued." });
//    //}
//}
