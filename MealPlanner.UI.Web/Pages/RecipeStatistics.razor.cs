using BlazorBootstrap;
using Common.Shared;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeStatistics
    {
        private PieChart favorites1Chart = default!;
        private PieChartOptions favorites1ChartOptions = default!;
        private ChartData favorites1ChartData = new();

        private PieChart favorites2Chart = default!;
        private PieChartOptions favorites2ChartOptions = default!;
        private ChartData favorites2ChartData = default!;

        private DoughnutChart favorites3Chart = default!;
        private DoughnutChartOptions favorites3ChartOptions = default!;
        private ChartData favorites3ChartData = default!;

        private DoughnutChart favorites4Chart = default!;
        private DoughnutChartOptions favorites4Options = default!;
        private ChartData favorites4ChartData = default!;

        private PieChart favorites5Chart = default!;
        private PieChartOptions favorites5ChartOptions = default!;
        private ChartData favorites5ChartData = new();

        private PieChart favorites6Chart = default!;
        private PieChartOptions favorites6ChartOptions = default!;
        private ChartData favorites6ChartData = new();

        private PieChart favorites7Chart = default!;
        private PieChartOptions favorites7ChartOptions = default!;
        private ChartData favorites7ChartData = new();

        public StatisticModel? FavoriteRecipes1 { get; set; }
        public StatisticModel? FavoriteRecipes2 { get; set; }
        public StatisticModel? FavoriteRecipes3 { get; set; }
        public StatisticModel? FavoriteRecipes4 { get; set; }
        public StatisticModel? FavoriteRecipes5 { get; set; }
        public StatisticModel? FavoriteRecipes6 { get; set; }
        public StatisticModel? FavoriteRecipes7 { get; set; }

        [Inject]
        public IStatisticsService? StatisticsService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            FavoriteRecipes1 = await StatisticsService!.GetFavoriteRecipesAsync("2");
            FavoriteRecipes2 = await StatisticsService!.GetFavoriteRecipesAsync("4");
            FavoriteRecipes3 = await StatisticsService!.GetFavoriteRecipesAsync("5");
            FavoriteRecipes4 = await StatisticsService!.GetFavoriteRecipesAsync("7");
            FavoriteRecipes5 = await StatisticsService!.GetFavoriteRecipesAsync("9");
            FavoriteRecipes6 = await StatisticsService!.GetFavoriteRecipesAsync("10");
            FavoriteRecipes7 = await StatisticsService!.GetFavoriteRecipesAsync("12");

            if (FavoriteRecipes1 != null && FavoriteRecipes1.Data != null)
            {
                favorites1ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes1), Datasets = GetDataSets(FavoriteRecipes1, ChartType.Pie) };
                favorites1ChartOptions = new();
                favorites1ChartOptions.Responsive = true;
                favorites1ChartOptions.Plugins.Title!.Text = FavoriteRecipes1.Title;
                favorites1ChartOptions.Plugins.Title.Display = true;
                favorites1ChartOptions.Plugins.Legend.Position = "right";
                await favorites1Chart.InitializeAsync(favorites1ChartData, favorites1ChartOptions);
            }

            if (FavoriteRecipes2 != null && FavoriteRecipes2.Data != null)
            {
                favorites2ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes2), Datasets = GetDataSets(FavoriteRecipes2, ChartType.Pie) };
                favorites2ChartOptions = new();
                favorites2ChartOptions.Responsive = true;
                favorites2ChartOptions.Plugins.Title!.Text = FavoriteRecipes2.Title;
                favorites2ChartOptions.Plugins.Title.Display = true;
                favorites2ChartOptions.Plugins.Legend.Position = "right";
                await favorites2Chart.InitializeAsync(favorites2ChartData, favorites2ChartOptions);
            }

            if (FavoriteRecipes3 != null && FavoriteRecipes3.Data != null)
            {
                favorites3ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes3), Datasets = GetDataSets(FavoriteRecipes3, ChartType.Doughnut) };
                favorites3ChartOptions = new();
                favorites3ChartOptions.Responsive = true;
                favorites3ChartOptions.Plugins.Title!.Text = FavoriteRecipes3.Title;
                favorites3ChartOptions.Plugins.Title.Display = true;
                favorites3ChartOptions.Plugins.Legend.Position = "right";
                await favorites3Chart.InitializeAsync(favorites3ChartData, favorites3ChartOptions);
            }

            if (FavoriteRecipes4 != null && FavoriteRecipes4.Data != null)
            {
                favorites4ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes4), Datasets = GetDataSets(FavoriteRecipes4, ChartType.Doughnut) };
                favorites4Options = new();
                favorites4Options.Responsive = true;
                favorites4Options.Plugins.Title!.Text = FavoriteRecipes4.Title;
                favorites4Options.Plugins.Title.Display = true;
                favorites4Options.Plugins.Legend.Position = "right";
                await favorites4Chart.InitializeAsync(favorites4ChartData, favorites4Options);
            }

            if (FavoriteRecipes5 != null && FavoriteRecipes5.Data != null)
            {
                favorites5ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes5), Datasets = GetDataSets(FavoriteRecipes5, ChartType.Pie) };
                favorites5ChartOptions = new();
                favorites5ChartOptions.Responsive = true;
                favorites5ChartOptions.Plugins.Title!.Text = FavoriteRecipes5.Title;
                favorites5ChartOptions.Plugins.Title.Display = true;
                favorites5ChartOptions.Plugins.Legend.Position = "right";
                await favorites5Chart.InitializeAsync(favorites5ChartData, favorites5ChartOptions);
            }

            if (FavoriteRecipes6 != null && FavoriteRecipes6.Data != null)
            {
                favorites6ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes6), Datasets = GetDataSets(FavoriteRecipes6, ChartType.Pie) };
                favorites6ChartOptions = new();
                favorites6ChartOptions.Responsive = true;
                favorites6ChartOptions.Plugins.Title!.Text = FavoriteRecipes6.Title;
                favorites6ChartOptions.Plugins.Title.Display = true;
                favorites6ChartOptions.Plugins.Legend.Position = "right";
                await favorites6Chart.InitializeAsync(favorites6ChartData, favorites6ChartOptions);
            }

            if (FavoriteRecipes7 != null && FavoriteRecipes7.Data != null)
            {
                favorites7ChartData = new ChartData { Labels = GetDataLabels(FavoriteRecipes7), Datasets = GetDataSets(FavoriteRecipes7, ChartType.Pie) };
                favorites7ChartOptions = new();
                favorites7ChartOptions.Responsive = true;
                favorites7ChartOptions.Plugins.Title!.Text = FavoriteRecipes7.Title;
                favorites7ChartOptions.Plugins.Title.Display = true;
                favorites7ChartOptions.Plugins.Legend.Position = "right";
                await favorites7Chart.InitializeAsync(favorites7ChartData, favorites7ChartOptions);
            }
        }

        private List<IChartDataset> GetDataSets(StatisticModel model, ChartType chartType)
        {
            var datasets = new List<IChartDataset>();
            if (model != null && model.Data != null)
            {
                string? label = model.Label;
                List<double> data = model!.Data!.Select(item => item.Value).ToList();
                List<string>?  backgroundColors = Extensions.GetBackgroundColors(model!.Data!.Count);

                switch (chartType)
                {
                    case ChartType.Line:
                        datasets.Add(new LineChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                        break;
                    case ChartType.Bar:
                        datasets.Add(new BarChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                        break;
                    case ChartType.Pie:
                        datasets.Add(new PieChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                        break;
                    case ChartType.Doughnut:
                        datasets.Add(new DoughnutChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                        break;
                }
            };
            return datasets;
        }

        private List<string> GetDataLabels(StatisticModel model)
        {
            if (model != null && model.Data != null)
            {
                return model!.Data!.Select(item => item.Key).ToList();
            }
            return new List<string>();
        }
    }
}
