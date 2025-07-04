using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpPost("login")]
        public async Task<LoginCommandResponse> LoginAsync(LoginModel model)
        {
            LoginCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPost("register")]
        public async Task<LoginCommandResponse> RegisterAsync(LoginModel model)
        {
            LoginCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }
    }
}
