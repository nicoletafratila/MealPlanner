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

        public IList<StatisticModel>? Statistics { get; set; } = new List<StatisticModel>();
        public PagedList<RecipeCategoryModel>? Categories { get; set; }

        [Inject]
        public IStatisticsService? StatisticsService { get; set; }

        [Inject] 
        public IRecipeCategoryService? CategoryService { get; set; }

        [Inject] 
        protected PreloadService PreloadService { get; set; } = default!;

        [Inject] 
        public ISessionStorageService? SessionStorage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            QueryParameters = new QueryParameters<RecipeCategoryModel>()
            {
                Filters = new List<FilterItem>(),
                Sorting = new List<SortingModel>() { new SortingModel() { PropertyName = "DisplaySequence", Direction = SortDirection.Ascending } },
                PageSize = 3,
                PageNumber = 1
            };
            await RefreshAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Statistics is not null && Statistics.Any())
            {
                var tasks = Statistics
                    .Where(s => s.Chart is not null && s.ChartData is not null && s.ChartOptions is not null)
                    .Select(s => s.Chart!.InitializeAsync(s.ChartData!, s.ChartOptions!));

                await Task.WhenAll(tasks);

                PreloadService.Hide();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task RefreshAsync()
        {
            Categories = await CategoryService!.SearchAsync(QueryParameters!);

            if (Categories?.Items is { Count: > 0 })
            {
                PreloadService.Show(SpinnerColor.Primary);
            }

            Statistics = await StatisticsService!.GetFavoriteRecipesAsync(Categories!.Items!);
            foreach (var item in Statistics)
            {
                item.GenerateChartData();
            }
            StateHasChanged();
        }

        private async Task OnPageChangedAsync(int pageNumber)
        {
            QueryParameters!.PageNumber = pageNumber;
            await RefreshAsync();
        }
    }
}
