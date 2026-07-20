namespace MealPlanner.UI.Mobile.Views.Controls
{
    public class SelectorItem(object value, string name, string? subtitle = null, string? imageUrl = null)
    {
        public object Value { get; } = value;

        public string Name { get; } = name;

        public string? Subtitle { get; } = subtitle;

        public string? ImageUrl { get; } = imageUrl;
    }
}
