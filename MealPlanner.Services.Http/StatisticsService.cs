using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Services;
using MealPlanner.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Core.Http
{
    public class StatisticsService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<StatisticsService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            MealPlannerControllers.StatisticsUrl
            ?? throw new ArgumentException("Statistics controller URL is not configured.");

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{MealPlannerControllers.FavoriteRecipesRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.CategoryIds] = string.Join(",", categoryIds) });
            return await GetAsync<IList<StatisticModel>>(url, cancellationToken);
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{MealPlannerControllers.FavoriteProductsRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.CategoryIds] = string.Join(",", categoryIds) });
            return await GetAsync<IList<StatisticModel>>(url, cancellationToken);
        }
    }
}
