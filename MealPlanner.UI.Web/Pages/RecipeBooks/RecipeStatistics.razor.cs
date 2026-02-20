using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipeStatistics
    {
        [Parameter]
        public QueryParameters<RecipeCategoryModel>? QueryParameters { get; set; } = new();

        public IList<StatisticModel> Statistics { get; private set; } = [];
        public PagedList<RecipeCategoryModel>? Categories { get; private set; }

        [Inject]
        public IStatisticsService StatisticsService { get; set; } = default!;

        [Inject]
        public IRecipeCategoryService CategoryService { get; set; } = default!;

        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            QueryParameters = new QueryParameters<RecipeCategoryModel>
            {
                Filters = [],
                Sorting =
                [
                    new SortingModel
                    {
                        PropertyName = "DisplaySequence",
                        Direction = SortDirection.Ascending
                    }
                ],
                PageSize = 3,
                PageNumber = 1
            };

            await RefreshAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Statistics.Any())
            {
                await InitializeChartsAsync(Statistics);
                PreloadService.Hide();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private static Task InitializeChartsAsync(IEnumerable<StatisticModel> statistics)
        {
            var tasks = statistics
                .Where(s => s.Chart is not null && s.ChartData is not null && s.ChartOptions is not null)
                .Select(s => s.Chart!.InitializeAsync(s.ChartData!, s.ChartOptions!));

            return Task.WhenAll(tasks);
        }

        private async Task RefreshAsync()
        {
            Categories = await CategoryService.SearchAsync(QueryParameters!) ?? new PagedList<RecipeCategoryModel>([], new Metadata { PageNumber = 1, PageSize = 1, TotalCount = 0 });
            if (Categories.Items is { Count: > 0 })
            {
                PreloadService.Show(SpinnerColor.Primary);
            }

            var items = Categories.Items ?? [];
            var data = await StatisticsService.GetFavoriteRecipesAsync(items) ?? [];

            foreach (var item in data)
            {
                item.GenerateChartData();
            }

            Statistics = data;
            StateHasChanged();
        }

        private async Task OnPageChangedAsync(int pageNumber)
        {
            QueryParameters!.PageNumber = pageNumber;
            await RefreshAsync();
        }
    }
}