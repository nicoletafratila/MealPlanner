using Microsoft.Extensions.Configuration;

namespace Common.Core.Tests
{
    public class TestApiConfig(IConfiguration configuration) : ApiConfig(configuration)
    {
        public override string? Name => "MyApi";
    }
}
