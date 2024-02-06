using System.Drawing;
using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeStatistics
    {
        private PieChart pieChart = default!;
        private PieChartOptions pieChartOptions = default!;
        private ChartData chartData = new ChartData();
        private List<string>? backgroundColors = new List<string>();

        public PagedList<RecipeModel>? Recipes { get; set; }

        public PagedList<MealPlanModel>? MealPlans { get; set; }

        public IList<EditMealPlanModel>? MealPlanWithRecipes { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService!.SearchAsync();
            Recipes = await RecipeService!.SearchAsync("2");
            MealPlanWithRecipes = new List<EditMealPlanModel>();
            foreach (var item in MealPlans!.Items!)
            {
                var mealPlan = await MealPlanService!.GetEditAsync(item.Id);
                MealPlanWithRecipes.Add(mealPlan!);
            }
            backgroundColors = GetBackgroundColors(Recipes!.Items!.Count);
        }

        protected async Task ShowAsync()
        {
            chartData = new ChartData { Labels = GetDataLabels(), Datasets = GetDataSets("Supe si ciorbe") };
            pieChartOptions = new();
            pieChartOptions.Responsive = true;
            pieChartOptions.Plugins.Title!.Text = "Favorite recipes";
            pieChartOptions.Plugins.Title.Display = true;
            pieChartOptions.Plugins.Legend.Position = "right";
            await pieChart.InitializeAsync(chartData, pieChartOptions);
        }

        private List<IChartDataset> GetDataSets(string categoryName)
        {
            var datasets = new List<IChartDataset>
            {
                GetPieChartDataset(categoryName)
            };
            return datasets;
        }

        private PieChartDataset GetPieChartDataset(string categoryName)
        {
            return new() { Label = categoryName, Data = GetData(), BackgroundColor = backgroundColors };
        }

        private List<double> GetData()
        {
            var data = new List<double>();

            if (Recipes != null && Recipes.Items != null)
            {
                foreach (var recipe in Recipes!.Items!.OrderBy(i => i.Name))
                {
                    var count = 0;
                    foreach (var item in MealPlanWithRecipes!)
                    {
                        count += item!.Recipes!.Count(i => i.Id == recipe.Id);
                    }
                    data.Add(count);
                }
            }

            return data;
        }

        private List<string> GetDataLabels()
        {
            if (Recipes != null && Recipes.Items != null)
            {
                return Recipes!.Items!.Select(x => x.Name!).Order().ToList();
            }
            return new List<string>();
        }

        private List<string> GetBackgroundColors(int counter)
        {
            var rand = new Random();
            var colors = new List<string>();
            for (var index = 0; index < counter; index++)
            {
                var color = string.Empty;
                while (!colors.Contains(color))
                {
                    color = "#" + Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)).Name.Substring(2);
                    if (!colors.Contains(color))
                        colors.Add(color);
                }
            }
            return colors;
        }
    }
}
