using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Log : Entity<int>
    {
        public string? Message { get; set; } = string.Empty;
        public string? MessageTemplate { get; set; } = string.Empty;
        public string? Level { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Exception { get; set; } = string.Empty;
        public string? LogEvent { get; set; } = string.Empty;
        [Column(TypeName = "xml")]
        public string? Properties { get; set; } = string.Empty;
    }
}
