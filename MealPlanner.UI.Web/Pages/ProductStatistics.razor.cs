using BlazorBootstrap;
using Common.Pagination;
using Common.Shared;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductStatistics
    {
        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        public IList<StatisticModel>? Statistics { get; set; }
        public PagedList<ProductCategoryModel>? Categories { get; set; }

        [Inject]
        public IStatisticsService? StatisticsService { get; set; }

        [Inject]
        public IProductCategoryService? CategoryService { get; set; }

        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            QueryParameters!.PageSize = 3;
            await RefreshAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Statistics != null && Statistics.Any())
            {
                foreach (var item in Statistics)
                {
                    await item.Chart!.InitializeAsync(item.ChartData!, item.ChartOptions!);
                }
                PreloadService.Hide();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task RefreshAsync()
        {
            PreloadService.Show(SpinnerColor.Primary);
            Statistics = new List<StatisticModel>();
            Categories = await CategoryService!.SearchAsync(QueryParameters!);
            Statistics = await StatisticsService!.GetFavoriteProductsAsync(Categories?.Items!);
            foreach (var item in Statistics!)
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
