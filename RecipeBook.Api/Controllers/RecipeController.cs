using AutoMapper;
using Common.Constants;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;
using System.Net.Http.Headers;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public RecipeController(IRecipeRepository recipeRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _recipeRepository = recipeRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<IList<RecipeModel>>> GetAll()
        {
            try
            {
                var results = await _recipeRepository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<RecipeModel>>(results).OrderBy(item => item.RecipeCategory!.DisplaySequence).ThenBy(item => item.Name);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RecipeModel>> GetById(int id)
        {
            try
            {
                var result = await _recipeRepository.GetByIdAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<RecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("edit/{id:int}")]
        public async Task<ActionResult<EditRecipeModel>> GetEdit(int id)
        {
            try
            {
                var result = await _recipeRepository.GetByIdIncludeIngredientsAsync(id);
                if (result == null) return NotFound();
                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditRecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search/{categoryid:int}")]
        public async Task<ActionResult<IList<RecipeModel>>> SearchByCategoryId(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest();

            try
            {
                var results = await _recipeRepository.SearchAsync(categoryId);
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
                await _recipeRepository.AddAsync(result);
                
                result = await _recipeRepository.GetByIdIncludeIngredientsAsync(result.Id);
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
                var oldModel = await _recipeRepository.GetByIdIncludeIngredientsAsync(model.Id);
                if (oldModel == null)
                {
                    return NotFound($"Could not find with id {model.Id}");
                }

                _mapper.Map(model, oldModel);
                await _recipeRepository.UpdateAsync(oldModel);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var itemToDelete = await _recipeRepository.GetByIdAsync(id);
            if (itemToDelete == null)
            {
                return NotFound($"Could not find with id {id}");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7249/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await client.GetFromJsonAsync<IList<MealPlanModel>>($"{ApiNames.MealPlanApi}/search/{id}");
                if (result != null && result.Any())
                {
                    return BadRequest($"The recipe you try to delete is used in meal plans and cannot be deleted.");
                }
            }

            await _recipeRepository.DeleteAsync(itemToDelete!);
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
