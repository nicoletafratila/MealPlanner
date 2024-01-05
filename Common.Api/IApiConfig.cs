namespace Common.Api
{
    public interface IApiConfig
    {
        public Uri? BaseUrl { get; set; }
        public int Timeout { get; set; }
        public string? Name { get; }
        public Dictionary<string, string>? Endpoints { get; set; }
    }
}
