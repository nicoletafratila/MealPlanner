using BlazorBootstrap;
using Common.Constants;

namespace Common.Models
{
    public class StatisticModel : BaseModel
    {
        public string? Title { get; set; }
        public string? Label { get; set; }
        public Dictionary<string, double>? Data { get; set; } = [];
        public DoughnutChart? Chart = new();
        public DoughnutChartOptions? ChartOptions = new();
        public ChartData? ChartData = new();

        public void GenerateChartData()
        {
            ChartData = new ChartData { Labels = GetDataLabels(), Datasets = GetDataSets() };
            ChartOptions = new();
            ChartOptions.Responsive = true;
            ChartOptions.Plugins.Title!.Text = Title;
            ChartOptions.Plugins.Title.Display = true;
            ChartOptions.Plugins.Legend.Position = "right";
        }

        private List<IChartDataset> GetDataSets()
        {
            var datasets = new List<IChartDataset>();
            if (Data != null)
            {
                List<double>? values = Data?.Select(item => item.Value).ToList();
                List<string>? backgroundColors = Colors.GetBackgroundColors(Data!.Count);
                datasets.Add(new DoughnutChartDataset() { Label = Label, Data = values, BackgroundColor = backgroundColors });

                //switch (chartType)
                //{
                //    case ChartType.Line:
                //        datasets.Add(new LineChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                //        break;
                //    case ChartType.Bar:
                //        datasets.Add(new BarChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                //        break;
                //    case ChartType.Pie:
                //        datasets.Add(new PieChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                //        break;
                //    case ChartType.Doughnut:
                //        datasets.Add(new DoughnutChartDataset() { Label = label, Data = data, BackgroundColor = backgroundColors });
                //        break;
                //}
            };
            return datasets;
        }

        private List<string>? GetDataLabels()
        {
            return Data?.Select(item => item.Key).ToList();
        }
    }
}
