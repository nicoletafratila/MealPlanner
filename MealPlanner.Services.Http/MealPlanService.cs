using System.Globalization;
using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace MealPlanner.Services.Http
{
    public class MealPlanService(HttpClient httpClient, ITokenProvider tokenProvider, IMemoryCache cache, ILogger<MealPlanService> logger)
        : ServiceBase(httpClient, tokenProvider), IMealPlanService
    {
        private readonly string _controller = MealPlannerControllers.MealPlanUrl;
        private static CancellationTokenSource _cacheToken = new();

        private static void InvalidateCache()
        {
            var old = Interlocked.Exchange(ref _cacheToken, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }

        public async Task<MealPlanEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<MealPlanEditModel>(url, cancellationToken);
        }

        public async Task<MealPlanModel?> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            var result = await SearchAsync(new QueryParameters<MealPlanModel>
            {
                Filters = CreateCurrentWeekFilters(),
                PageNumber = 1,
                PageSize = 1,
                Sorting = [new SortingModel { PropertyName = nameof(MealPlanModel.CreatedAt), Direction = SortDirection.Descending }]
            }, cancellationToken) ?? new PagedList<MealPlanModel>([], new Metadata());

            return result.Items.FirstOrDefault();
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(Guid mealPlanId, Guid shopId, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.ShoppingListProductsRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.MealPlanId] = mealPlanId.ToString(), [ApiQueryParams.ShopId] = shopId.ToString() });
            return await GetAsync<IList<ShoppingListProductEditModel>>(url, cancellationToken);
        }

        public async Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters<MealPlanModel>? queryParameters = null, CancellationToken cancellationToken = default)
        {
            var userId = JwtUserIdExtractor.GetUserId(await TokenProvider.GetTokenAsync(cancellationToken));
            var cacheKey = SearchCacheKeyBuilder.Build("mealPlans", queryParameters, userId);
            if (cache.TryGetValue(cacheKey, out PagedList<MealPlanModel>? cached))
            {
                return cached;
            }

            var result = await SearchAsync(_controller, queryParameters, cancellationToken);

            if (result is not null)
            {
                var opts = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .AddExpirationToken(new CancellationChangeToken(_cacheToken.Token));
                cache.Set(cacheKey, result, opts);
            }

            return result;
        }

        public async Task<CommandResponse?> AddAsync(MealPlanEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PostAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MealPlan AddAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(MealPlanEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await PutAsync(_controller, model, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MealPlan UpdateAsync failed. Model {@Model}", model);
                throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try
            {
                var r = await DeleteAsync(url, cancellationToken);
                InvalidateCache();
                return r;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MealPlan DeleteAsync failed. Id {Id}", id);
                throw;
            }
        }

        public string GetMenuName(string menuName)
        {
            var now = DateTime.Now;
            var calendar = CultureInfo.InvariantCulture.Calendar;
            int week = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"{menuName} {now.Year}/{week}";
        }

        private List<FilterItem> CreateCurrentWeekFilters()
        {
            var weekStart = GetCurrentWeekStart();
            var weekEnd = weekStart.AddDays(7);

            return
            [
                new FilterItem(nameof(MealPlanEditModel.CreatedAt), weekStart.ToString(), FilterOperator.GreaterThanOrEquals, StringComparison.OrdinalIgnoreCase),
                new FilterItem(nameof(MealPlanEditModel.CreatedAt), weekEnd.ToString(), FilterOperator.LessThan, StringComparison.OrdinalIgnoreCase)
            ];
        }

        private static DateTime GetCurrentWeekStart()
        {
            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return today.AddDays(-diff).Date;
        }
    }
}
