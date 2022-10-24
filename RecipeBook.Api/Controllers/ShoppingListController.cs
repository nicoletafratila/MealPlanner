using AutoMapper;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Api.Data.Repositories;

namespace RecipeBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListController : ControllerBase
    {
        private readonly IMealPlanRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShoppingListController(IMealPlanRepository repository, IMapper mapper, LinkGenerator linkGenerator,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListModel>> Get(int id)
        {
            try
            {
                var results = await _repository.GetByIdAsyncIncludeRecipes(id);

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<ShoppingListModel>(results));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
