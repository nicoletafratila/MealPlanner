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
        [HttpGet("edit/{id:int}")]
        public async Task<MealPlanEditModel> GetEditAsync(int id)
        {
            GetEditMealPlanQuery query = new()
            {
                Id = id
            };
            return await mediator.Send(query);
        }

        [HttpGet("shoppingListProducts/{mealPlanId:int}/{shopId:int}")]
        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int mealPlanId, int shopId)
        {
            GetShoppingListProductsQuery query = new()
            {
                MealPlanId = mealPlanId,
                ShopId= shopId
            };
            return await mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<MealPlanModel>> SearchAsync([FromQuery] string? filters, [FromQuery] string? sorting, [FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            SearchQuery query = new()
            {
                QueryParameters = new QueryParameters<MealPlanModel>()
                {
                    Filters = !string.IsNullOrWhiteSpace(filters) ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters) : null,
                    Sorting = !string.IsNullOrWhiteSpace(sorting) ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting) : null,
                    PageSize = int.Parse(pageSize!),
                    PageNumber = int.Parse(pageNumber!)
                }
            };
            return await mediator.Send(query);
        }

        [HttpGet("search/{recipeId:int}")]
        public async Task<IList<MealPlanModel>> SearchByRecipeIdAsync(int recipeId)
        {
            SearchByRecipeIdQuery query = new()
            {
                RecipeId = recipeId
            };
            return await mediator.Send(query);
        }

        [HttpPost]
        public async Task<CommandResponse?> PostAsync(MealPlanEditModel model)
        {
            AddCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(MealPlanEditModel model)
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