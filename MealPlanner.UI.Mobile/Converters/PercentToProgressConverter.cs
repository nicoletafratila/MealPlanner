using System.Globalization;

namespace MealPlanner.UI.Mobile.Converters
{
    public class PercentToProgressConverter : IValueConverter
    {
        public static readonly PercentToProgressConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is int percent ? percent / 100.0 : 0.0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
