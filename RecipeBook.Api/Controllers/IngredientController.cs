using AutoMapper;
using Common.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public IngredientController(IIngredientRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IngredientModel>>> GetAll()
        {
            try
            {
                var results = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<IngredientModel>>(results).OrderBy(item => item.IngredientCategory.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EditIngredientModel>> Get(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);

                if (result == null) return NotFound();

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditIngredientModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("category/{categoryid:int}")]
        public async Task<ActionResult<IList<IngredientModel>>> Search(int categoryId)
        {
            try
            {
                var results = await _repository.SearchAsync(categoryId);
                var mappedResults = _mapper.Map<IList<IngredientModel>>(results).OrderBy(item => item.IngredientCategory.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EditIngredientModel>> Post(EditIngredientModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError("Name", "The name shouldn't be empty");
                }
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string location = _linkGenerator.GetPathByAction("Get", "Recipe", new { id = model.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }

                var result = _mapper.Map<Ingredient>(model);
                await _repository.AddAsync(result);
                return Created(location, _mapper.Map<EditIngredientModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult<EditIngredientModel>> Put(EditIngredientModel model)
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
                return _mapper.Map<EditIngredientModel>(oldModel);
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
            if (itemToDelete != null)
            {
                await _repository.DeleteAsync(itemToDelete);
            }
        }
    }
}
