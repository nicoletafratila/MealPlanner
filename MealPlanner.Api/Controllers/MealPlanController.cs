using AutoMapper;
using Common.Data.Entities;
using Common.Pagination;
using MealPlanner.Api.Features.MealPlan.Queries.GetMealPlan;
using MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlans;
using MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlansByRecipeId;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealPlanController : ControllerBase
    {
        private readonly IMealPlanRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        private readonly ISender _mediator;

        public MealPlanController(ISender mediator, IMealPlanRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
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
        public async Task<ActionResult<EditMealPlanModel>> Post(EditMealPlanModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var result = _mapper.Map<MealPlan>(model);
                await _repository.AddAsync(result);
                
                result = await _repository.GetByIdIncludeRecipesAsync(result.Id);
                string? location = _linkGenerator.GetPathByAction("GetEdit", "MealPlan", new { id = result!.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }
                return Created(location, _mapper.Map<EditMealPlanModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(EditMealPlanModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var oldModel = await _repository.GetByIdAsync(model.Id);
                if (oldModel == null)
                {
                    return NotFound($"Could not find with id {model.Id}");
                }

                _mapper.Map(model, oldModel);
                await _repository.UpdateAsync(oldModel);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var itemToDelete = await _repository.GetByIdAsync(id);
            if (itemToDelete == null)
            {
                NotFound($"Could not find with id {id}");
                return;
            }

            await _repository.DeleteAsync(itemToDelete);
        }
    }
}