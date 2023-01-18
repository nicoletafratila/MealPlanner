using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeCategoryController : ControllerBase
    {
        private readonly IRecipeCategoryRepository _repository;
        private readonly IMapper _mapper;

        public RecipeCategoryController(IRecipeCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IList<RecipeCategoryModel>>> Get()
        {
            try
            {
                var results = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<RecipeCategoryModel>>(results).OrderBy(r => r.DisplaySequence);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
