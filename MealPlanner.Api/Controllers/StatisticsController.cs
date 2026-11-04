using Common.Http;
using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class StatisticsController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpGet("favorite/recipes")]
        public async Task<ActionResult<IList<StatisticModel>>> SearchFavoriteRecipesAsync(
            [FromQuery] string? categoryIds,
            [FromQuery] string? filters,
            [FromQuery] string? sorting,
            CancellationToken cancellationToken)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchRecipes.SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader),
                Filters = DeserializeFilters(filters),
                Sorting = DeserializeSorting(sorting)
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("favorite/products")]
        public async Task<ActionResult<IList<StatisticModel>?>> SearchFavoriteProductsAsync(
            [FromQuery] string? categoryIds,
            [FromQuery] string? filters,
            [FromQuery] string? sorting,
            CancellationToken cancellationToken)
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            var query = new Features.Statistics.Queries.SearchProducts.SearchQuery
            {
                CategoryIds = categoryIds,
                AuthToken = HttpClientExtensions.GetCleanToken(authHeader),
                Filters = DeserializeFilters(filters),
                Sorting = DeserializeSorting(sorting)
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        private static IEnumerable<FilterItem>? DeserializeFilters(string? filters) =>
            !string.IsNullOrWhiteSpace(filters)
                ? JsonConvert.DeserializeObject<IEnumerable<FilterItem>>(filters)
                : null;

        private static IEnumerable<SortingModel>? DeserializeSorting(string? sorting) =>
            !string.IsNullOrWhiteSpace(sorting)
                ? JsonConvert.DeserializeObject<IEnumerable<SortingModel>>(sorting)
                : null;
    }
}