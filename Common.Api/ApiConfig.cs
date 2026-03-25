using Microsoft.Extensions.Configuration;

namespace Common.Api
{
    public class ApiConfig : IApiConfig
    {
        public Uri? BaseUrl { get; set; }
        public int Timeout { get; set; }
        public virtual string? Name => string.Empty;
        public Dictionary<string, string>? Controllers { get; set; } = [];

        public ApiConfig(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var sectionName = Name;

            if (string.IsNullOrWhiteSpace(sectionName))
            {
                configuration.Bind(this);
            }
            else
            {
                configuration.GetSection(sectionName).Bind(this);
            }
        }
    }
}
