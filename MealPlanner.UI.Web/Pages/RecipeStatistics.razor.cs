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
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Statistics = new List<StatisticModel>();
            var categories = await RecipeCategoryService!.GetAllAsync();
            foreach (var category in categories!)
            {
                var favorites = await StatisticsService!.GetFavoriteRecipesAsync(category.Id);
                if (favorites != null && favorites.Data != null)
                {
                    favorites.ChartData = new ChartData { Labels = favorites.GetDataLabels(), Datasets = favorites.GetDataSets(ChartType.Doughnut) };
                    favorites.ChartOptions = new();
                    favorites.ChartOptions.Responsive = true;
                    favorites.ChartOptions.Plugins.Title!.Text = favorites.Title;
                    favorites.ChartOptions.Plugins.Title.Display = true;
                    favorites.ChartOptions.Plugins.Legend.Position = "right";
                    Statistics.Add(favorites);
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            foreach (var item in Statistics!)
            {
                await item.Chart!.InitializeAsync(item.ChartData!, item.ChartOptions!);
            }
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
