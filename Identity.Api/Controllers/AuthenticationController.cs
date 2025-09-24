using System.Security.Claims;
using Common.Models;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(ISender mediator) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<CommandResponse> LoginAsync(LoginModel model)
        {
            LoginCommand command = new() { Model = model };
            if (await mediator.Send(command) is LoginCommandResponse response && response.Succeeded)
            {
                var claimObjects = response.Claims?.Select(c => new Claim(c.Key, c.Value));
                var identity = new ClaimsIdentity(claimObjects, IdentityConstants.ApplicationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false, // Change to true for "Remember Me"
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    }
                );
                return response;
            }
            return CommandResponse.Failed("Invalid credentials.");
        }

        [HttpPost("register")]
        public async Task<CommandResponse> RegisterAsync(LoginModel model)
        {
            LoginCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }
    }
}

