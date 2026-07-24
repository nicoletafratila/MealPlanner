namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public class CategoryStatisticModel
    {
        public string Title { get; init; } = string.Empty;
        public double TotalValue { get; init; }
        public double SharePercentage { get; init; }
        public IReadOnlyList<RankedStatisticItemModel> Items { get; init; } = [];
    }
}
