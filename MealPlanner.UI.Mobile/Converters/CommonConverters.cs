using System.Globalization;

namespace MealPlanner.UI.Mobile.Converters
{
    public class StringNotNullConverter : IValueConverter
    {
        public static readonly StringNotNullConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            !string.IsNullOrWhiteSpace(value as string);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class NotNullConverter : IValueConverter
    {
        public static readonly NotNullConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not null;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class InvertBoolConverter : IValueConverter
    {
        public static readonly InvertBoolConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && !b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && !b;
    }

    public class BoolToStringConverter : IValueConverter
    {
        public static readonly BoolToStringConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var parts = parameter?.ToString()?.Split('|') ?? [];
            return value is true
                ? (parts.Length > 0 ? parts[0] : "Yes")
                : (parts.Length > 1 ? parts[1] : "No");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class CollectedToDecorConverter : IValueConverter
    {
        public static readonly CollectedToDecorConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is true ? TextDecorations.Strikethrough : TextDecorations.None;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ZeroToBoolConverter : IValueConverter
    {
        public static readonly ZeroToBoolConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is int i && i == 0;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class FractionToStarConverter : IValueConverter
    {
        public static readonly FractionToStarConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            new GridLength(value is double d ? Math.Max(d, 0.01) : 0.01, GridUnitType.Star);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
