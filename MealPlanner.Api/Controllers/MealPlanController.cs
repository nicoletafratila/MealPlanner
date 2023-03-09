using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
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

        public MealPlanController(IMealPlanRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IList<MealPlanModel>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<MealPlanModel>>(result).OrderBy(r => r.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditMealPlanModel>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditMealPlanModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
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
                
                result = await _repository.GetByIdAsyncIncludeRecipesAsync(result.Id);
                string location = _linkGenerator.GetPathByAction("Get", "MealPlan", new { id = result.Id });
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