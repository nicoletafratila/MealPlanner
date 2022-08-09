using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Data;
using RecipeBook.Api.Data.Entities;
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
        public async Task<ActionResult<RecipeModel>> Get(int id)
        {
            try
            {
                var result = await _repository.GetAsync(id);

                if (result == null) return NotFound();

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<RecipeModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<RecipeModel>> Post(RecipeModel model)
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
                _repository.Add(result);

                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<RecipeModel>(result));
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpPut]
        public async Task<ActionResult<RecipeModel>> Put(RecipeModel model)
        {
            if (model == null)
                return BadRequest();

            try
            {
                var oldModel = await _repository.GetAsync(model.Id);
                if (oldModel == null)
                {
                    return NotFound($"Could not find with id {model.Id}");
                }

                _mapper.Map(model, oldModel);

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<RecipeModel>(oldModel);
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }
    }
}
