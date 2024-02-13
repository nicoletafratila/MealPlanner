using BlazorBootstrap;
using Common.Shared;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductStatistics
    {
        public IList<StatisticModel>? Statistics { get; set; }

        [Inject]
        public IStatisticsService? StatisticsService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            PreloadService.Show(SpinnerColor.Primary);

            Statistics = new List<StatisticModel>();
            var categories = await ProductCategoryService!.GetAllAsync();
            foreach (var category in categories!)
            {
                var favorites = await StatisticsService!.GetFavoriteProductsAsync(category.Id);
                favorites!.GenerateChartData();
                Statistics.Add(favorites!);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            foreach (var item in Statistics!)
            {
                await item.Chart!.InitializeAsync(item.ChartData!, item.ChartOptions!);
            }

            if (Statistics != null && Statistics.Any())
            {
                PreloadService.Hide();
            }
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
