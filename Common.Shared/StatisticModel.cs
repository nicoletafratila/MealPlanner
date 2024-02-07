namespace Common.Shared
{
    public class StatisticModel : BaseModel
    {
        public string? Title { get; set; }
        public string? Label { get; set; }
        public Dictionary<string, double>? Data { get; set; }
    }
}
