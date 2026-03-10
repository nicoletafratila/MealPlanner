using Common.Api;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class StatisticsController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet("favoriterecipes")]
        public async Task<ActionResult<IList<StatisticModel>>> SearchFavoriteRecipesAsync(
            [FromQuery] string? categoryIds,
            CancellationToken cancellationToken)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchRecipes.SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("favoriteproducts")]
        public async Task<ActionResult<IList<StatisticModel>?>> SearchFavoriteProductsAsync(
            [FromQuery] string? categoryIds,
            CancellationToken cancellationToken)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchProducts.SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}