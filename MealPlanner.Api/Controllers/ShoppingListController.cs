using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public ShoppingListController(IShoppingListRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditShoppingListModel>> GetById(int id)
        {
            try
            {
                //var result = await _repository.GetByIdAsyncIncludeRecipesAsync(id);
                //var mappedResults = _mapper.Map<ShoppingListModel>(result);
                //return StatusCode(StatusCodes.Status200OK, mappedResults);
                var result = await _repository.GetByIdAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditShoppingListModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EditShoppingListModel>> Post(EditShoppingListModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var result = _mapper.Map<ShoppingList>(model);
                await _repository.AddAsync(result);

                result = await _repository.GetByIdAsyncIncludeProductsAsync(result.Id);
                string? location = _linkGenerator.GetPathByAction("GetById", "MealPlan", new { id = result!.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }
                return Created(location, _mapper.Map<EditShoppingListModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(EditShoppingListModel model)
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
    }
}
