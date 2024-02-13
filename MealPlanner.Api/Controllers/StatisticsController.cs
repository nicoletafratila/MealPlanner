using Common.Shared;
using MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts;
using MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly ISender _mediator;

        public StatisticsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("favoriterecipes")]
        public async Task<StatisticModel?> SearchFavoriteRecipes([FromQuery] string? categoryId)
        {
            GetFavoriteRecipesQuery query = new()
            {
                CategoryId = categoryId
            };
            return await _mediator.Send(query);
        }

        [HttpGet("favoriteproducts")]
        public async Task<StatisticModel?> SearchFavoriteProducts([FromQuery] string? categoryId)
        {
            GetFavoriteProductsQuery query = new()
            {
                CategoryId = categoryId
            };
            return await _mediator.Send(query);
        }
    }
}
