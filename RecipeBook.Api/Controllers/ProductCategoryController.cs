using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryRepository _repository;
        private readonly IMapper _mapper;

        public ProductCategoryController(IProductCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IList<ProductCategoryModel>>> GetAll()
        {
            try
            {
                var results = await _repository.GetAllAsync();
                var mappedResults = _mapper.Map<IList<ProductCategoryModel>>(results).OrderBy(r => r.DisplaySequence);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
