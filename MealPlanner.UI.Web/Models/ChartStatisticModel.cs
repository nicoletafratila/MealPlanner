using BlazorBootstrap;
using Common.Constants;
using Common.Models;

namespace MealPlanner.UI.Web.Models
{
    public class ChartStatisticModel : StatisticModel
    {
        public DoughnutChart? Chart { get; set; }
        public DoughnutChartOptions? ChartOptions { get; set; } = new();
        public ChartData? ChartData { get; set; } = new();

        public ChartStatisticModel()
        {
        }

        public ChartStatisticModel(StatisticModel source)
        {
            ArgumentNullException.ThrowIfNull(source);

            Index = source.Index;
            IsSelected = source.IsSelected;
            Title = source.Title;
            Label = source.Label;
            Data = source.Data ?? new Dictionary<string, double?>();
        }

        public void GenerateChartData()
        {
            ChartData = new ChartData
            {
                Labels = GetDataLabels(),
                Datasets = GetDataSets()
            };

            ChartOptions = new DoughnutChartOptions
            {
                Responsive = true
            };

            ChartOptions.Plugins.Title!.Text = Title;
            ChartOptions.Plugins.Title.Display = true;
            ChartOptions.Plugins.Legend.Position = "bottom";
            ChartOptions.Plugins.Legend.Reverse = true;
            ChartOptions.Plugins.Legend.FullSize = true;
        }

        private List<IChartDataset> GetDataSets()
        {
            var datasets = new List<IChartDataset>();
            if (Data != null && Data.Count > 0)
            {
                var values = Data.Select(item => item.Value).ToList();
                var backgroundColors = Colors.GetBackgroundColors(Data.Count, System.Drawing.Color.DarkOliveGreen);

                datasets.Add(new DoughnutChartDataset
                {
                    Label = Label,
                    Data = values,
                    BackgroundColor = backgroundColors
                });
            }
            return datasets;
        }

        private List<string>? GetDataLabels()
        {
            return Data?.Select(item => item.Key).ToList();
        }
    }
}
