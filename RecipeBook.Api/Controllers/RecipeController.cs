using System.Text.Json;
using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Recipe.Commands.Add;
using RecipeBook.Api.Features.Recipe.Commands.Delete;
using RecipeBook.Api.Features.Recipe.Commands.Update;
using RecipeBook.Api.Features.Recipe.Queries.GetById;
using RecipeBook.Api.Features.Recipe.Queries.GetEdit;
using RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts;
using RecipeBook.Api.Features.Recipe.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController(ISender mediator) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<RecipeModel> GetByIdAsync(int id)
        {
            GetByIdQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("edit/{id:int}")]
        public async Task<RecipeEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("shoppingListProducts/{recipeId:int}/{shopId:int}")]
        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int recipeId, int shopId)
        {
            GetShoppingListProductsQuery query = new()
            {
                RecipeId = recipeId,
                ShopId = shopId
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sortString, [FromQuery] string? sortDirection, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonSerializer.Deserialize<IEnumerable<FilterItem>>(filters) : null,
                    SortString = sortString,
                    SortDirection = sortDirection == SortDirection.Ascending.ToString() ? SortDirection.Ascending : SortDirection.Descending,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpPost]
        public async Task<CommandResponse?> PostAsync(RecipeEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(RecipeEditModel model)
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
