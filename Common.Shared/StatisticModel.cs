using BlazorBootstrap;
using Common.Constants;

namespace Common.Shared
{
    public class StatisticModel : BaseModel
    {
        public string? Title { get; set; }
        public string? Label { get; set; }
        public Dictionary<string, double>? Data { get; set; }
        public DoughnutChart? Chart = new();
        public DoughnutChartOptions? ChartOptions = new();
        public ChartData? ChartData = new();

        public List<IChartDataset> GetDataSets(ChartType chartType)
        {
            var datasets = new List<IChartDataset>();
            if (Data != null)
            {
                string? label = Label;
                List<double> data = Data!.Select(item => item.Value).ToList();
                List<string>? backgroundColors = Colors.GetBackgroundColors(Data!.Count);

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

        public List<string> GetDataLabels()
        {
            if (Data != null)
            {
                return Data!.Select(item => item.Key).ToList();
            }
            return new List<string>();
        }
    }
}
