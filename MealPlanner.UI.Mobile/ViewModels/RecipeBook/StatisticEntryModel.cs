namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public class StatisticEntryModel
    {
        public string GroupTitle { get; init; } = string.Empty;
        public bool IsGroupHeader { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public double Value { get; init; }
        public double BarFraction { get; init; }
    }
}
