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

using System.Security.Claims;
using Common.Data.Entities;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        // 1. Authenticate
        var signInResult = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (signInResult.Succeeded)
        {
            // 2. (Optionally) Reload page/signal re-auth via frontend if using Blazor Server

            // 3. Diagnostic: Confirm Set-Cookie will be issued
            if (!HttpContext.Response.Headers.ContainsKey("Set-Cookie"))
                _logger.LogInformation("Expecting Set-Cookie header; Identity should issue automatically.");

            // 4. Return a response for the Blazor client—your UI
            return Ok(new { message = "Login successful.", username = model.Username, succeeded = true });
        }
        return Unauthorized(new { message = "User/password not found." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logout successful." });
    }

    //[HttpPost("diagnostic-cookie")]
    //public async Task<IActionResult> DiagnosticCookie()
    //{
    //    await HttpContext.SignInAsync(
    //        CookieAuthenticationDefaults.AuthenticationScheme,
    //        new ClaimsPrincipal(new ClaimsIdentity(
    //            new[] { new Claim(ClaimTypes.Name, "debuguser") },
    //            CookieAuthenticationDefaults.AuthenticationScheme
    //        ))
    //    );
    //    return Ok(new { message = "Manual cookie issued." });
    //}
}
