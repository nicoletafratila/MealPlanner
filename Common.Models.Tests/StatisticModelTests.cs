using BlazorBootstrap;

namespace Common.Models.Tests
{
    [TestFixture]
    public class StatisticModelTests
    {
        [Test]
        public void GenerateChartData_PopulatesLabelsAndDataset_FromData()
        {
            // Arrange
            var model = new StatisticModel
            {
                Title = "Favorite Recipes",
                Label = "Recipes",
                Data = new Dictionary<string, double?>
                {
                    ["Pasta"] = 10,
                    ["Salad"] = 5,
                    ["Soup"] = 3
                }
            };

            // Act
            model.GenerateChartData();

            // Assert: labels
            Assert.That(model.ChartData, Is.Not.Null);
            Assert.That(model.ChartData!.Labels, Is.Not.Null);
            Assert.That(model.ChartData.Labels, Is.EquivalentTo(["Pasta", "Salad", "Soup"]));

            // Assert: datasets
            Assert.That(model.ChartData.Datasets, Is.Not.Null);
            Assert.That(model.ChartData.Datasets!.Count, Is.EqualTo(1));

            var ds = model.ChartData.Datasets[0] as DoughnutChartDataset;
            Assert.That(ds, Is.Not.Null);
            Assert.That(ds!.Label, Is.EqualTo("Recipes"));
            Assert.That(ds.Data, Is.EquivalentTo(new double?[] { 10, 5, 3 }));
            Assert.That(ds.BackgroundColor, Is.Not.Null);
            Assert.That(ds.BackgroundColor!.Count, Is.EqualTo(3));
        }

        [Test]
        public void GenerateChartData_SetsChartOptions_TitleAndLegend()
        {
            // Arrange
            var model = new StatisticModel
            {
                Title = "Favorite Products",
                Label = "Products",
                Data = new Dictionary<string, double?>
                {
                    ["Tomatoes"] = 4,
                    ["Cheese"] = 6
                }
            };

            // Act
            model.GenerateChartData();

            // Assert: options
            Assert.That(model.ChartOptions, Is.Not.Null);
            Assert.That(model.ChartOptions!.Responsive, Is.True);

            var plugins = model.ChartOptions.Plugins;
            Assert.That(plugins, Is.Not.Null);
            Assert.That(plugins!.Title, Is.Not.Null);
            Assert.That(plugins.Title!.Display, Is.True);
            Assert.That(plugins.Title.Text, Is.EqualTo("Favorite Products"));

            Assert.That(plugins.Legend, Is.Not.Null);
            Assert.That(plugins.Legend!.Position, Is.EqualTo("bottom"));
            Assert.That(plugins.Legend.Reverse, Is.True);
            Assert.That(plugins.Legend.FullSize, Is.True);
        }

        [Test]
        public void GenerateChartData_WithEmptyData_ProducesNoDatasets()
        {
            // Arrange
            var model = new StatisticModel
            {
                Title = "Empty",
                Label = "None",
                Data = []
            };

            // Act
            model.GenerateChartData();

            // Assert
            Assert.That(model.ChartData, Is.Not.Null);
            Assert.That(model.ChartData!.Datasets, Is.Not.Null);
            Assert.That(model.ChartData.Datasets!.Count, Is.EqualTo(0));
            Assert.That(model.ChartData.Labels, Is.Not.Null);
            Assert.That(model.ChartData.Labels!.Count, Is.EqualTo(0));
        }
    }
}
