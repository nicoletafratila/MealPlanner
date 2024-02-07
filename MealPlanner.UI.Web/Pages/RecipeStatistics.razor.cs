using BlazorBootstrap;
using Common.Shared;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeStatistics
    {
        private PieChart pieChart = default!;
        private PieChartOptions pieChartOptions = default!;
        private ChartData chartData = new();

        public StatisticModel? FavoriteRecipes { get; set; }

        [Inject]
        public IStatisticsService? StatisticsService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            FavoriteRecipes = await StatisticsService!.GetFavoriteRecipesAsync("2");

            if (FavoriteRecipes != null && FavoriteRecipes.Data != null)
            {
                chartData = new ChartData { Labels = GetDataLabels(), Datasets = GetDataSets() };
                pieChartOptions = new();
                pieChartOptions.Responsive = true;
                pieChartOptions.Plugins.Title!.Text = FavoriteRecipes.Title;
                pieChartOptions.Plugins.Title.Display = true;
                pieChartOptions.Plugins.Legend.Position = "right";
                await pieChart.InitializeAsync(chartData, pieChartOptions);
            }
        }

        private List<IChartDataset> GetDataSets()
        {
            var datasets = new List<IChartDataset>();
            if (FavoriteRecipes != null && FavoriteRecipes.Data != null)
            {
                string? label = FavoriteRecipes.Label;
                List<double> data = FavoriteRecipes!.Data!.Select(item => item.Value).ToList();
                List<string>?  backgroundColors = Extensions.GetBackgroundColors(FavoriteRecipes!.Data!.Count);

                datasets.Add(new PieChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
            };
            return datasets;
        }

        private List<string> GetDataLabels()
        {
            if (FavoriteRecipes != null && FavoriteRecipes.Data != null)
            {
                return FavoriteRecipes!.Data!.Select(item => item.Key).ToList();
            }
            return new List<string>();
        }
    }
}
