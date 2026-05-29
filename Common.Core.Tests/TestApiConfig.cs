using Microsoft.Extensions.Configuration;

namespace Common.Core.Tests
{
    public sealed class TestApiConfig(IConfiguration configuration) : ApiConfig(configuration)
    {
        public override string? Name => "MyApi";
    }
}
