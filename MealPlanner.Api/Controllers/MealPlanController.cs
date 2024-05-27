using Common.Pagination;
using MealPlanner.Api.Features.MealPlan.Commands.Add;
using MealPlanner.Api.Features.MealPlan.Commands.Delete;
using MealPlanner.Api.Features.MealPlan.Commands.Update;
using MealPlanner.Api.Features.MealPlan.Queries.GetAll;
using MealPlanner.Api.Features.MealPlan.Queries.GetEdit;
using MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts;
using MealPlanner.Api.Features.MealPlan.Queries.Search;
using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealPlanController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        [HttpGet("edit/{id:int}")]
        public async Task<EditMealPlanModel> GetEdit(int id)
        {
            GetEditMealPlanQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("shoppingListProducts/{mealPlanId:int}/{shopId:int}")]
        public async Task<IList<ShoppingListProductModel>?> GetShoppingListProducts(int mealPlanId, int shopId)
        {
            GetShoppingListProductsQuery query = new()
            {
                MealPlanId = mealPlanId,
                ShopId= shopId
            };
            return await _mediator.Send(query);
        }

        [HttpGet]
        public async Task<IList<MealPlanModel>> GetAll()
        {
            return await _mediator.Send(new GetAllQuery());
        }

        [HttpGet("search")]
        public async Task<PagedList<MealPlanModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search/{recipeId:int}")]
        public async Task<IList<MealPlanModel>> SearchByRecipeId(int recipeId)
        {
            SearchByRecipeIdQuery query = new()
            {
                RecipeId = recipeId
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddCommandResponse> Post(EditMealPlanModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateCommandResponse> Put(EditMealPlanModel model)
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