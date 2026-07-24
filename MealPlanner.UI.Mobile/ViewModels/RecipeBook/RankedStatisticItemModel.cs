namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public class RankedStatisticItemModel
    {
        public int Rank { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public double Value { get; init; }
        public double BarFraction { get; init; }
    }
}
