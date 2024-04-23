﻿namespace Common.Data.Entities
{
    public class Log : Entity<int>
    {
        public string? Message { get; set; } = string.Empty;
        public string? MessageTemplate { get; set; } = string.Empty;
        public string? Level { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public string? Exception { get; set; } = string.Empty;
        public string? Properties { get; set; } = string.Empty;
    }
}