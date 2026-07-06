using System.Globalization;
using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Http
{
    public class MealPlanService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<MealPlanService> logger)
        : ServiceBase(httpClient, tokenProvider), IMealPlanService
    {
        private readonly string _controller = MealPlannerControllers.MealPlanUrl;

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
                PageSize = int.MaxValue
            }, cancellationToken) ?? new PagedList<MealPlanModel>([], new Metadata());

            return result.Items.Count != 0 ? result.Items.OrderByDescending(x => x.CreatedAt).FirstOrDefault() : null;
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(Guid mealPlanId, Guid shopId, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.ShoppingListProductsRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.MealPlanId] = mealPlanId.ToString(), [ApiQueryParams.ShopId] = shopId.ToString() });
            return await GetAsync<IList<ShoppingListProductEditModel>>(url, cancellationToken);
        }

        public Task<PagedList<MealPlanModel>?> SearchAsync(QueryParameters<MealPlanModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<CommandResponse?> AddAsync(MealPlanEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                return await PostAsync(_controller, model, cancellationToken);
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
                return await PutAsync(_controller, model, cancellationToken);
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
                return await DeleteAsync(url, cancellationToken);
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

        private DateTime GetCurrentWeekStart()
        {
            var today = DateTime.Today;
            var firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            var diff = (7 + (today.DayOfWeek - firstDay)) % 7;
            return today.AddDays(-diff).Date;
        }
    }
}
