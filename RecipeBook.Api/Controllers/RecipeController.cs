using BlazorBootstrap;
using Common.Api;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class RecipeController(ISender mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<RecipeModel> GetByIdAsync(int id)
        {
            GetByIdQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("edit")]
        public async Task<RecipeEditModel> GetEditAsync(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("shoppingListProducts")]
        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int recipeId, int shopId)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            GetShoppingListProductsQuery query = new()
            {
                RecipeId = recipeId,
                ShopId = shopId,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sorting, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters<RecipeModel>()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters) : null,
                    Sorting = !string.IsNullOrWhiteSpace(sorting) ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting) : null,
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

        [HttpDelete]
        public async Task<CommandResponse?> DeleteAsync(int id)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            DeleteCommand command = new()
            {
                Id = id,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };
            return await mediator.Send(command);
        }
    }
}
