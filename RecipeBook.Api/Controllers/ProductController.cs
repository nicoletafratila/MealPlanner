using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RecipeBook.Api.Features.Product.Commands.Add;
using RecipeBook.Api.Features.Product.Commands.Delete;
using RecipeBook.Api.Features.Product.Commands.Update;
using RecipeBook.Api.Features.Product.Queries.GetEdit;
using RecipeBook.Api.Features.Product.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class ProductController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet("edit")]
        public async Task<ActionResult<ProductEditModel>> GetEditAsync(
            [FromQuery] int id,
            CancellationToken cancellationToken)
        {
            var query = new GetEditQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedList<ProductModel>>> SearchAsync(
            [FromQuery] string? filters,
            [FromQuery] string? sorting,
            [FromQuery] string? pageSize,
            [FromQuery] string? pageNumber,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(pageSize, out var size) || size <= 0 ||
                !int.TryParse(pageNumber, out var number) || number <= 0)
            {
                return BadRequest("pageSize and pageNumber must be positive integers.");
            }

            var filterItems = !string.IsNullOrWhiteSpace(filters)
                ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters)
                : null;

            var sortingItems = !string.IsNullOrWhiteSpace(sorting)
                ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting)
                : null;

            var qp = new QueryParameters<ProductModel>
            {
                Filters = filterItems,
                Sorting = sortingItems,
                PageSize = size,
                PageNumber = number
            };

            var query = new SearchQuery
            {
                QueryParameters = qp
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CommandResponse?>> PostAsync(
            [FromBody] ProductEditModel model,
            CancellationToken cancellationToken)
        {
            var command = new AddCommand { Model = model };
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<CommandResponse?>> PutAsync(
            [FromBody] ProductEditModel model,
            CancellationToken cancellationToken)
        {
            var command = new UpdateCommand { Model = model };
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpDelete]
        public async Task<ActionResult<CommandResponse?>> DeleteAsync(
            [FromQuery] int id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteCommand { Id = id };
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
    }
}