using System.Security.Claims;
using Common.Models;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Api.Features.Authentication.Commands.Logout;
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
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpPost("login")]
        public async Task<CommandResponse?> LoginAsync(
            [FromBody] LoginModel model,
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand { Model = model };

            var result = await _mediator.Send(command, cancellationToken);

            if (result is not LoginCommandResponse loginResponse || !loginResponse.Succeeded)
            {
                return CommandResponse.Failed("Invalid credentials.");
            }

            var claimObjects = (loginResponse.Claims ?? Enumerable.Empty<KeyValuePair<string, string>>())
                .Select(c => new Claim(c.Key, c.Value));

            var identity = new ClaimsIdentity(claimObjects, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                new AuthenticationProperties
                {
                    // For now, always non-persistent; you can wire RememberLogin here later
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                });

            return loginResponse;
        }

        [HttpPost("logout")]
        public async Task<CommandResponse?> LogoutAsync(
            CancellationToken cancellationToken)
        {
            var command = new LogoutCommand();
            return await _mediator.Send(command, cancellationToken);
        }

        /// <summary>
        /// NOTE: As implemented, this just forwards to the same LoginCommand.
        /// If you add a proper RegistrationCommand later, wire it here.
        /// </summary>
        [HttpPost("register")]
        public async Task<CommandResponse?> RegisterAsync(
            [FromBody] LoginModel model,
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand
            {
                Model = model
            };

            return await _mediator.Send(command, cancellationToken);
        }
    }
}