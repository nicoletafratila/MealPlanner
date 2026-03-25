using Microsoft.Extensions.Configuration;

namespace Common.Api.Tests
{
    public sealed class TestApiConfig(IConfiguration configuration) : ApiConfig(configuration)
    {
        public override string? Name => "MyApi";
    }
}
