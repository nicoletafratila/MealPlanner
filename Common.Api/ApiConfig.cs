namespace Common.Api
{
    public class ApiConfig : IApiConfig
    {
        public Uri? BaseUrl { get; set; }
        public int Timeout { get; set; }
        public virtual string? Name => string.Empty;
        public Dictionary<string, string> Endpoints { get ; set; }

        public ApiConfig(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            configuration.Bind(Name, this);
        }
    }
}
