using Common.Pagination;
using MealPlanner.Api.Features.MealPlan.Commands.AddMealPlan;
using MealPlanner.Api.Features.MealPlan.Commands.DeleteMealPlan;
using MealPlanner.Api.Features.MealPlan.Commands.UpdateMealPlan;
using MealPlanner.Api.Features.MealPlan.Queries.GetMealPlan;
using MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlans;
using MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlansByRecipeId;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealPlanController : ControllerBase
    {
        private readonly ISender _mediator;

        public MealPlanController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("edit/{id:int}")]
        public async Task<EditMealPlanModel> GetEdit(int id)
        {
            GetEditMealPlanQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<MealPlanModel>> Search([FromQuery] QueryParameters? queryParameters)
        {
            SearchMealPlansQuery query = new()
            {
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search/{recipeId:int}")]
        public async Task<IList<MealPlanModel>> SearchByRecipeId(int recipeId)
        {
            SearchMealPlansByRecipeIdQuery query = new()
            {
                RecipeId = recipeId
            };
            return await _mediator.Send(query);
        }

        [HttpPost]
        public async Task<AddMealPlanCommandResponse> Post(EditMealPlanModel model)
        {
            AddMealPlanCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpPut]
        public async Task<UpdateMealPlanCommandResponse> Put(EditMealPlanModel model)
        {
            UpdateMealPlanCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task<DeleteMealPlanCommandResponse> Delete(int id)
        {
            DeleteMealPlanCommand command = new()
            {
                Id = id
            };
            return await _mediator.Send(command);
        }
    }
}