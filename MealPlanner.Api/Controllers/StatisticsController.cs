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
        public async Task<IList<StatisticModel>> SearchFavoriteRecipesAsync([FromQuery] string? categories)
        {
            var categoryIds = string.IsNullOrWhiteSpace(categories)
                ? new List<int>()
                : categories
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(part => int.TryParse(part, out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchRecipes.SearchQuery
            {
                Categories = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };

            return await mediator.Send(query);
        }

        [HttpGet("favoriteproducts")]
        public async Task<IList<StatisticModel>?> SearchFavoriteProductsAsync([FromQuery] string? categories)
        {
            var categoryIds = string.IsNullOrWhiteSpace(categories)
                ? new List<int>()
                : categories
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(part => int.TryParse(part, out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchProducts.SearchQuery
            {
                Categories = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader)
            };

            return await mediator.Send(query);
        }
    }
}
