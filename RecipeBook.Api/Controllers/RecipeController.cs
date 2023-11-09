using AutoMapper;
using Common.Api;
using Common.Constants;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Features.Recipe.Commands.AddRecipe;
using RecipeBook.Api.Features.Recipe.Queries.GetEditRecipe;
using RecipeBook.Api.Features.Recipe.Queries.GetRecipe;
using RecipeBook.Api.Features.Recipe.Queries.SearchRecipes;
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
        private readonly IApiConfig _mealPlannerApiConfig;
        private readonly ISender _mediator;

        public RecipeController(ISender mediator, IRecipeRepository recipeRepository, IServiceProvider serviceProvider, IMapper mapper)
        {
            _recipeRepository = recipeRepository;
            _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
            _mapper = mapper;
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        public async Task<RecipeModel> GetById(int id)
        {
            GetRecipeQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("edit/{id:int}")]
        public async Task<ActionResult<EditRecipeModel>> GetEdit(int id)
        {
            GetEditRecipeQuery query = new()
            {
                Id = id
            };
            return await _mediator.Send(query);
        }

        [HttpGet("search")]
        public async Task<PagedList<RecipeModel>> Search([FromQuery] string? categoryId, [FromQuery] QueryParameters? queryParameters)
        {
            SearchRecipesQuery query = new()
            {
                CategoryId = categoryId,
                QueryParameters = queryParameters
            };
            return await _mediator.Send(query);
        }

        //[HttpPost]
        //public async Task<ActionResult<EditRecipeModel>> Post(EditRecipeModel model)
        //{
        //    if (model == null)
        //        return BadRequest();

        //    try
        //    {
        //        var result = _mapper.Map<Recipe>(model);
        //        await _recipeRepository.AddAsync(result);

        //        result = await _recipeRepository.GetByIdIncludeIngredientsAsync(result.Id);
        //        string? location = _linkGenerator.GetPathByAction("GetById", "Recipe", new { id = result!.Id });
        //        if (string.IsNullOrWhiteSpace(location))
        //        {
        //            return BadRequest("Could not use current id");
        //        }
        //        return Created(location, _mapper.Map<EditRecipeModel>(result));
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
        //    }
        //}
        [HttpPost]
        public async Task<AddRecipeCommandResponse> PostAsync(EditRecipeModel model)
        {
            AddRecipeCommand command = new()
            {
                Model = model
            };
            return await _mediator.Send(command);
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
                client.BaseAddress = new Uri("https://localhost:7249/");//_mealPlannerApiConfig!.BaseUrl;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await client.GetFromJsonAsync<IList<MealPlanModel>>($"{_mealPlannerApiConfig.Endpoints[ApiEndpointNames.MealPlanApi]}/search/{id}");
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
