using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Features.MealPlan.Commands.Add;
using MealPlanner.Api.Features.MealPlan.Commands.Delete;
using MealPlanner.Api.Features.MealPlan.Commands.Update;
using MealPlanner.Api.Features.MealPlan.Queries.GetEdit;
using MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts;
using MealPlanner.Api.Features.MealPlan.Queries.Search;
using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class MealPlanController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet("edit")]
        public async Task<ActionResult<MealPlanEditModel>> GetEditAsync(
            [FromQuery] int id,
            CancellationToken cancellationToken)
        {
            var query = new GetEditQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("shoppingListProducts")]
        public async Task<ActionResult<IList<ShoppingListProductEditModel>?>> GetShoppingListProductsAsync(
            [FromQuery] int mealPlanId,
            [FromQuery] int shopId,
            CancellationToken cancellationToken)
        {
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = mealPlanId,
                ShopId = shopId
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedList<MealPlanModel>>> SearchAsync(
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

            var qp = new QueryParameters<MealPlanModel>
            {
                Filters = filterItems,
                Sorting = sortingItems,
                PageSize = size,
                PageNumber = number
            };

            var query = new SearchQuery { QueryParameters = qp };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("searchbyid")]
        public async Task<ActionResult<IList<MealPlanModel>>> SearchByRecipeIdAsync(
            [FromQuery] int recipeId,
            CancellationToken cancellationToken)
        {
            var query = new SearchByRecipeIdQuery { RecipeId = recipeId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CommandResponse?>> PostAsync(
            [FromBody] MealPlanEditModel model,
            CancellationToken cancellationToken)
        {
            var command = new AddCommand { Model = model };
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<CommandResponse?>> PutAsync(
            [FromBody] MealPlanEditModel model,
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