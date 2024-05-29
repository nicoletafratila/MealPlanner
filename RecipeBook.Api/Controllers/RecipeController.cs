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
        private readonly ISender _mediator = mediator;

        [HttpGet("{id:int}")]
        public async Task<RecipeModel> GetById(int id)
        {
            GetByIdQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("edit/{id:int}")]
        public async Task<EditRecipeModel> GetEdit(int id)
        {
            GetEditQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("shoppingListProducts/{recipeId:int}/{shopId:int}")]
        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProducts(int recipeId, int shopId)
        {
            GetShoppingListProductsQuery query = new()
            {
                RecipeId = recipeId,
                ShopId = shopId
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeModel>> Search([FromQuery] string? categoryId, [FromQuery] QueryParameters? queryParameters)
        {
            SearchQuery query = new()
            {
                CategoryId = categoryId,
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddCommandResponse> PostAsync(EditRecipeModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(EditRecipeModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteCommandResponse> Delete(int id)
        {
            DeleteCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}
