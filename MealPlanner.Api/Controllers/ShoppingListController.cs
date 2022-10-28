using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Api.Services;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
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
        private IQuantityCalculator _quantityCalculator;

        public ShoppingListController(IMealPlanRepository repository, IMapper mapper, IQuantityCalculator quantityCalculator, LinkGenerator linkGenerator,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _quantityCalculator = quantityCalculator;
            _linkGenerator = linkGenerator;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListModel>> Get(int id)
        {
            try
            {
                var mealPlan = await _repository.GetByIdAsyncIncludeRecipes(id);
                var result = _mapper.Map<ShoppingListModel>(mealPlan);
                result.Ingredients = _quantityCalculator.CalculateQuantities(result.Ingredients);
                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
