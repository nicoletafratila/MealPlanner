using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientCategoryController : ControllerBase
    {
        private readonly IIngredientCategoryRepository _repository;
        private readonly IMapper _mapper;

        public IngredientCategoryController(IIngredientCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientCategoryModel>>> Get()
        {
            try
            {
                var results = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IEnumerable<IngredientCategoryModel>>(results).OrderBy(r => r.DisplaySequence);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
