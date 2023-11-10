using Common.Pagination;
using MealPlanner.Api.Features.ShoppingList.Commands.AddShoppingList;
using MealPlanner.Api.Features.ShoppingList.Commands.DeleteShoppingList;
using MealPlanner.Api.Features.ShoppingList.Commands.UpdateShoppingList;
using MealPlanner.Api.Features.ShoppingList.Queries.GetEditShoppingList;
using MealPlanner.Api.Features.ShoppingList.Queries.SearchShoppingLists;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListController : ControllerBase
    {
        private readonly ISender _mediator;

        public ShoppingListController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("edit/{id:int}")]
        public async Task<EditShoppingListModel> GetEdit(int id)
        {
            GetEditShoppingListQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<ShoppingListModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchShoppingListsQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<EditShoppingListModel?> Post([FromBody] int id)
        {
            AddShoppingListCommand command = new()
            {
                MealPlanId = id
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateShoppingListCommandResponse> Put(EditShoppingListModel model)
        {
            UpdateShoppingListCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteShoppingListCommandResponse> Delete(int id)
        {
            DeleteShoppingListCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
