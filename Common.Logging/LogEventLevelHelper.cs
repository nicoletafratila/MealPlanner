using Serilog.Events;

namespace Common.Logging
{
    public static class LogEventLevelHelper
    {
        public static string GetBootstrapUIClass(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var level = StringToEnum(value);
            return GetBootstrapUIClass(level);
        }

        public static string GetBootstrapUIClass(LogEventLevel level)
            => level switch
            {
                LogEventLevel.Verbose or LogEventLevel.Debug or LogEventLevel.Information => "info",
                LogEventLevel.Warning => "warning",
                LogEventLevel.Error or LogEventLevel.Fatal => "danger",
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Not a valid LogEventLevel.")
            };

        public static LogEventLevel StringToEnum(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var level))
            {
                return level;
            }

            throw new ArgumentException($"Value '{value}' is not a valid LogEventLevel.", nameof(value));
        }
    }
}