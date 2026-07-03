namespace Common.Models
{
    public class StatisticModel : BaseModel
    {
        public string? Title { get; set; }
        public string? Label { get; set; }
        public IDictionary<string, double?> Data { get; set; } = new Dictionary<string, double?>();
    }
}
