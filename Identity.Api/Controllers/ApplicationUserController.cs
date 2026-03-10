using Common.Models;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class ApplicationUserController(ISender mediator) : ControllerBase
    {
        [HttpGet("edit")]
        public async Task<ApplicationUserEditModel> GetEditAsync(
            [FromQuery] string username,
            CancellationToken cancellationToken)
        {
            GetEditQuery query = new()
            {
                Name = username
            };

            return await mediator.Send(query, cancellationToken);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(
            [FromBody] ApplicationUserEditModel model,
            CancellationToken cancellationToken)
        {
            UpdateCommand command = new()
            {
                Model = model
            };

            return await mediator.Send(command, cancellationToken);
        }
    }
}