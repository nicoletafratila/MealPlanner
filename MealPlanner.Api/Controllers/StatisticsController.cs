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
        [HttpGet("favoriterecipes")]
        public async Task<IList<StatisticModel>> SearchFavoriteRecipesAsync([FromQuery] string? categoryIds)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchRecipes.SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };

            return await mediator.Send(query);
        }

        [HttpGet("favoriteproducts")]
        public async Task<IList<StatisticModel>?> SearchFavoriteProductsAsync([FromQuery] string? categoryIds)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchProducts.SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };

            return await mediator.Send(query);
        }
    }
}
