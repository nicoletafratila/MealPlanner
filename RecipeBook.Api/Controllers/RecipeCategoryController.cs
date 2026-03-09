using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RecipeBook.Api.Features.RecipeCategory.Commands.Add;
using RecipeBook.Api.Features.RecipeCategory.Commands.Delete;
using RecipeBook.Api.Features.RecipeCategory.Commands.Update;
using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit;
using RecipeBook.Api.Features.RecipeCategory.Queries.Search;
using RecipeBook.Api.Features.RecipeCategory.Queries.SearchByCategories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class RecipeCategoryController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet("edit")]
        public async Task<ActionResult<RecipeCategoryEditModel>> GetEditAsync([FromQuery] int id)
        {
            var query = new GetEditQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedList<RecipeCategoryModel>>> SearchAsync(
            [FromQuery] string? filters,
            [FromQuery] string? sorting,
            [FromQuery] string? pageSize,
            [FromQuery] string? pageNumber)
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

            var qp = new QueryParameters<RecipeCategoryModel>
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

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("searchbycategories")]
        public async Task<ActionResult<IList<RecipeCategoryModel>>> SearchByCategoriesAsync(
            [FromQuery] string categoryIds)
        {
            var query = new SearchByCategoriesQuery
            {
                CategoryIds = categoryIds
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CommandResponse?>> PostAsync([FromBody] RecipeCategoryEditModel model)
        {
            var command = new AddCommand { Model = model };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<CommandResponse?>> PutAsync([FromBody] RecipeCategoryEditModel model)
        {
            var command = new UpdateCommand { Model = model };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut("updateAll")]
        public async Task<ActionResult<CommandResponse?>> PutAllAsync([FromBody] IList<RecipeCategoryModel> models)
        {
            var command = new UpdateAllCommand { Models = models };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete]
        public async Task<ActionResult<CommandResponse?>> DeleteAsync([FromQuery] int id)
        {
            var command = new DeleteCommand { Id = id };
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}