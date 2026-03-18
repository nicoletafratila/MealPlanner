namespace Common.Models
{
    public sealed class LogModel
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public string MessageTemplate { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Exception { get; set; } = string.Empty;

        public string Properties { get; set; } = string.Empty;

        /// <summary>
        /// True when an exception has been logged.
        /// </summary>
        public bool HasException => !string.IsNullOrWhiteSpace(Exception);

        public override string ToString()
            => $"[{Timestamp:O}] {Level}: {Message}";
    }
}