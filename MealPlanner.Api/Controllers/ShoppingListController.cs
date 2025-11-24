using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Features.ShoppingList.Commands.Add;
using MealPlanner.Api.Features.ShoppingList.Commands.Delete;
using MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList;
using MealPlanner.Api.Features.ShoppingList.Commands.Update;
using MealPlanner.Api.Features.ShoppingList.Queries.GetEdit;
using MealPlanner.Api.Features.ShoppingList.Queries.Search;
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
    public class ShoppingListController(ISender mediator) : ControllerBase
    {
        [HttpGet("edit/{id:int}")]
        public async Task<ShoppingListEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ShoppingListModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sorting, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters<ShoppingListModel>()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters) : null,
                    Sorting = !string.IsNullOrWhiteSpace(sorting) ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting) : null,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpPost("makeShoppingList")]
        public async Task<ShoppingListEditModel?> MakeShoppingListAsync(ShoppingListCreateModel model)
        {
            MakeShoppingListCommand command = new()
            {
                MealPlanId = model.MealPlanId,
                ShopId = model.ShopId
            };
            return await mediator.Send(command);
        }

        [HttpPost]
        public async Task<CommandResponse?> PostAsync(ShoppingListEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(ShoppingListEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await mediator.Send(command);
        }
    }
}
