using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RecipeBook.Api.Controllers.Resources;
using RecipeBook.Api.Features.Job.Queries.GetStatus;
using RecipeBook.Api.Features.Job.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class JobController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet("{jobId}/status")]
        public async Task<ActionResult<JobStatusModel?>> GetStatusAsync(
            [FromRoute] Guid jobId,
            CancellationToken cancellationToken)
        {
            var query = new GetStatusQuery(jobId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("search")]
        [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin")]
        public async Task<ActionResult<PagedList<JobModel>>> SearchAsync(
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

            var qp = new QueryParameters<JobModel>
            {
                Filters = filterItems,
                Sorting = sortingItems!,
                PageSize = size,
                PageNumber = number
            };

            var query = new SearchQuery { QueryParameters = qp };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
