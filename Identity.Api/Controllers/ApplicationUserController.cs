using Common.Models;
using Common.Pagination;
using Identity.Api.Controllers.Resources;
using Identity.Api.Features.ApplicationUser.Commands.Unlock;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;
using Identity.Api.Features.ApplicationUser.Queries.Search;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class ApplicationUserController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        /// <summary>
        /// Searches users with optional filtering, sorting, and pagination.
        /// </summary>
        [HttpGet("search")]
        [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin")]
        public async Task<ActionResult<PagedList<ApplicationUserModel>>> SearchAsync(
            [FromQuery] string? filters,
            [FromQuery] string? sorting,
            [FromQuery] string? pageSize,
            [FromQuery] string? pageNumber,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(pageSize, out var size) || size <= 0 ||
                !int.TryParse(pageNumber, out var number) || number <= 0)
            {
                return BadRequest(ControllerMessages.InvalidPaginationParameters);
            }

            var filterItems = !string.IsNullOrWhiteSpace(filters)
                ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters)
                : null;

            var sortingItems = !string.IsNullOrWhiteSpace(sorting)
                ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting)
                : null;

            var qp = new QueryParameters<ApplicationUserModel>
            {
                Filters = filterItems,
                Sorting = sortingItems!,
                PageSize = size,
                PageNumber = number
            };

            var result = await _mediator.Send(new SearchQuery { QueryParameters = qp }, cancellationToken);
            return Ok(result);
        }

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
                return BadRequest(ControllerMessages.UsernameRequired);
            }

            var query = new GetEditQuery
            {
                Name = username
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (string.IsNullOrWhiteSpace(result.Username))
            {
                return NotFound(string.Format(ControllerMessages.UserNotFound, username));
            }

            return Ok(result);
        }

        /// <summary>
        /// Unlocks a locked-out user and resets their failed access count.
        /// </summary>
        [HttpPost("unlock")]
        [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin")]
        public async Task<ActionResult<CommandResponse?>> UnlockAsync(
            [FromBody] UnlockCommand command,
            CancellationToken cancellationToken)
        {
            if (command is null || string.IsNullOrWhiteSpace(command.UserId))
                return BadRequest(ControllerMessages.ModelRequired);

            var response = await _mediator.Send(command, cancellationToken);

            if (response is null)
                return StatusCode(StatusCodes.Status500InternalServerError, ControllerMessages.UnknownError);

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
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
                return BadRequest(ControllerMessages.ModelRequired);
            }

            var command = new UpdateCommand
            {
                Model = model
            };

            var response = await _mediator.Send(command, cancellationToken);

            if (response is null)
                return StatusCode(StatusCodes.Status500InternalServerError, ControllerMessages.UnknownError);

            if (!response.Succeeded)
                return BadRequest(response);

            return Ok(response);
        }
    }
}