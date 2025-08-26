using Common.Models;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class AuthenticationController(ISender mediator) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<CommandResponse> LoginAsync(LoginModel model)
        {
            LoginCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPost("register")]
        public async Task<LoginCommandResponse> RegisterAsync(LoginModel model)
        {
            LoginCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }
    }
}

