using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace Common.Api
{
    public class IdentityApiConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.Identity;

        public IdentityApiConfig(IConfiguration configuration) : base(configuration)
        {
            Controllers = new Dictionary<string, string>
            {
                [IdentityControllers.Authentication] = "api/authentication",
                [IdentityControllers.User] = "api/user"
            };
        }
    }
}
