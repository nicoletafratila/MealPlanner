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
        public async Task<IList<StatisticModel>?> SearchFavoriteRecipes()
        {
            return await _mediator.Send(new GetFavoriteRecipesQuery());
        }

        [HttpGet("favoriteproducts")]
        public async Task<IList<StatisticModel>?> SearchFavoriteProducts()
        {
            return await _mediator.Send(new GetFavoriteProductsQuery());
        }
    }
}
