using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Api.Data.Repositories;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecipeController(IRecipeRepository repository, IMapper mapper, LinkGenerator linkGenerator,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeModel>>> Get()
        {
            try
            {
                var results = await _repository.GetAllAsync();

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<IEnumerable<RecipeModel>>(results));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditRecipeModel>> Get(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsyncIncludeIngredients(id);

                if (result == null) return NotFound();

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditRecipeModel>(result));
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

                var result = _mapper.Map<Recipe>(model);
                await _repository.AddAsync(result);
                return Created(location, _mapper.Map<EditRecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPut]
        public async Task<ActionResult<EditRecipeModel>> Put(EditRecipeModel model)
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
                return _mapper.Map<EditRecipeModel>(oldModel);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
