namespace Common.Api
{
    public interface IApiConfig
    {
        Uri? BaseUrl { get; set; }
        int Timeout { get; set; }
        string? Name { get; }
        Dictionary<string, string>? Endpoints { get; set; }
    }
}
