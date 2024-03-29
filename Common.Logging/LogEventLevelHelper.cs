using Serilog.Events;

namespace Common.Logging
{
    public static class LogEventLevelHelper
    {
        public static string GetBootstrapUIClass(string value)
        {
            var level = StringToEnum(value);
            return level switch
            {
                LogEventLevel.Verbose or LogEventLevel.Debug or LogEventLevel.Information => "info",
                LogEventLevel.Warning => "warning",
                LogEventLevel.Error or LogEventLevel.Fatal => "danger",
                _ => throw new Exception("not valid logeventlevel")
            };
        }

        public static LogEventLevel StringToEnum(string value)
        {
            return value switch
            {
                "Verbose" => LogEventLevel.Verbose,
                "Debug" => LogEventLevel.Debug,
                "Information" => LogEventLevel.Information,
                "Warning" => LogEventLevel.Warning,
                "Error" => LogEventLevel.Error,
                "Fatal" => LogEventLevel.Fatal,
                _ => throw new Exception("not valid logeventlevel")
            };
        }
    }
}
