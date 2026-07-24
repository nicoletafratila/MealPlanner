using System.Globalization;

namespace MealPlanner.UI.Mobile.Converters
{
    public class RankToBadgeColorConverter : IValueConverter
    {
        public static readonly RankToBadgeColorConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value switch
            {
                1 => Color.FromArgb("#FFD700"),
                2 => Color.FromArgb("#C0C0C0"),
                3 => Color.FromArgb("#CD7F32"),
                _ => Color.FromArgb("#9E9E9E")
            };

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
