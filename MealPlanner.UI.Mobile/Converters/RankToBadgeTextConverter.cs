using System.Globalization;

namespace MealPlanner.UI.Mobile.Converters
{
    public class RankToBadgeTextConverter : IValueConverter
    {
        public static readonly RankToBadgeTextConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                int rank => rank.ToString(culture),
                _ => string.Empty
            };

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
