using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Shared.Models;

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

        [HttpGet]
        public async Task<ActionResult<IList<ShoppingListModel>>> GetAll()
        {
            try
            {
                var results = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<ShoppingListModel>>(results).OrderBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListModel>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<ShoppingListModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<ActionResult<EditShoppingListModel>> GetEdit(int id)
        {
            try
            {
                var result = await _repository.GetByIdIncludeProductsAsync(id);
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

                result = await _repository.GetByIdIncludeProductsAsync(result.Id);
                string? location = _linkGenerator.GetPathByAction("GetById", "ShoppingList", new { id = result!.Id });
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
