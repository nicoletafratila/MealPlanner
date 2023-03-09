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
        private IQuantityCalculator _quantityCalculator;

        public ShoppingListController(IMealPlanRepository repository, IMapper mapper, IQuantityCalculator quantityCalculator)
        {
            _repository = repository;
            _mapper = mapper;
            _quantityCalculator = quantityCalculator;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListModel>> Get(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsyncIncludeRecipesAsync(id);
                var mappedResults = _mapper.Map<ShoppingListModel>(result);
                mappedResults.Ingredients = _quantityCalculator.CalculateQuantities(mappedResults.Ingredients);
                return StatusCode(StatusCodes.Status200OK, mappedResults);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
