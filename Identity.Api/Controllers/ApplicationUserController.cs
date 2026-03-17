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
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>
        /// Gets user details for editing by username.
        /// </summary>
        [HttpGet("edit")]
        public async Task<ActionResult<ApplicationUserEditModel>> GetEditAsync(
            [FromQuery] string username,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username is required.");
            }

            var query = new GetEditQuery
            {
                Name = username
            };

            var result = await _mediator.Send(query, cancellationToken);

            // Handler already returns an empty model when user is not found,
            // but you might want to surface 404 in the API:
            if (string.IsNullOrWhiteSpace(result.Username))
            {
                return NotFound($"User '{username}' was not found.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Updates an application user.
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<CommandResponse?>> PutAsync(
            [FromBody] ApplicationUserEditModel model,
            CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return BadRequest("Model is required.");
            }

            var command = new UpdateCommand
            {
                Model = model
            };

            var response = await _mediator.Send(command, cancellationToken);

            if (response is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Unknown error.");

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }
    }
}