using AutoMapper;
using Common.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public RecipeController(IRecipeRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IList<RecipeModel>>> GetAll()
        {
            try
            {
                var results = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<RecipeModel>>(results).OrderBy(item => item.RecipeCategory!.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeModel>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<RecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<ActionResult<EditRecipeModel>> GetEdit(int id)
        {
            try
            {
                var result = await _repository.GetByIdIncludeIngredientsAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditRecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("category/{categoryid:int}")]
        public async Task<ActionResult<IList<RecipeModel>>> Search(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest();

            try
            {
                var results = await _repository.SearchAsync(categoryId);
                var mappedResults = _mapper.Map<IList<RecipeModel>>(results).OrderBy(item => item.RecipeCategory!.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EditRecipeModel>> Post(EditRecipeModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var result = _mapper.Map<Recipe>(model);
                await _repository.AddAsync(result);
                
                result = await _repository.GetByIdIncludeIngredientsAsync(result.Id);
                string? location = _linkGenerator.GetPathByAction("GetById", "Recipe", new { id = result!.Id });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current id");
                }
                return Created(location, _mapper.Map<EditRecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(EditRecipeModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var oldModel = await _repository.GetByIdIncludeIngredientsAsync(model.Id);
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
