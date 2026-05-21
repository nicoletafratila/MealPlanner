using System.Security.Claims;
using Common.Models;
using Identity.Api.Features.Authentication.Commands.ConfirmEmail;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Api.Features.Authentication.Commands.Logout;
using Identity.Api.Features.Authentication.Commands.Register;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(ISender mediator, IConfiguration configuration) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        [HttpPost("login")]
        public async Task<CommandResponse?> LoginAsync(
            [FromBody] LoginModel model,
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand { Model = model };

            var result = await _mediator.Send(command, cancellationToken);

            if (result is not LoginCommandResponse loginResponse || !loginResponse.Succeeded)
            {
                return CommandResponse.Failed(result!.Message!);
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

        [HttpPost("register")]
        public async Task<CommandResponse?> RegisterAsync(
            [FromBody] RegistrationModel model,
            CancellationToken cancellationToken)
        {
            var command = new RegisterCommand { Model = model };
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync(
            [FromQuery] string userId,
            [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            var uiBaseUrl = _configuration["MealPlannerWeb:BaseUrl"] ?? "https://localhost:7093";
            var command = new ConfirmEmailCommand { UserId = userId, Token = token };
            var result = await _mediator.Send(command, cancellationToken);

            return result?.Succeeded == true
                ? Redirect($"{uiBaseUrl}/identities/login?emailConfirmed=true")
                : Redirect($"{uiBaseUrl}/identities/login?emailConfirmationFailed=true");
        }
    }
}