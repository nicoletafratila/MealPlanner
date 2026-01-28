using BlazorBootstrap;
using Common.Constants;

namespace Common.Models
{
    public class StatisticModel : BaseModel
    {
        public string? Title { get; set; }
        public string? Label { get; set; }
        public Dictionary<string, double?>? Data { get; set; } = [];
        public DoughnutChart? Chart { get; set; }
        public DoughnutChartOptions? ChartOptions { get; set; } = new();
        public ChartData? ChartData { get; set; } = new();

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
