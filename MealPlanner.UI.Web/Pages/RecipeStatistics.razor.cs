using BlazorBootstrap;
using Common.Shared;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeStatistics
    {
        public IList<StatisticModel>? Statistics { get; set; }

        [Inject]
        public IStatisticsService? StatisticsService { get; set; }

        [Inject]
        protected PreloadService PreloadService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            PreloadService.Show(SpinnerColor.Primary);
            Statistics = await StatisticsService!.GetFavoriteRecipesAsync();
            foreach (var item in Statistics!)
            {
                item.GenerateChartData();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Statistics != null && Statistics.Any())
            {
                foreach (var item in Statistics!)
                {
                    await item.Chart!.InitializeAsync(item.ChartData!, item.ChartOptions!);
                }
                PreloadService.Hide();
            }
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
