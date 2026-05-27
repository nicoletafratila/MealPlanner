using Common.Api;
using Identity.Shared.Constants;

namespace Identity.Api
{
    public class IdentityApiConfig : ApiConfig
    {
        public override string? Name => ApiConfigNames.Identity;

        public IdentityApiConfig(IConfiguration configuration) : base(configuration)
        {
            Controllers = new Dictionary<string, string>
            {
                [IdentityControllers.Authentication] = "api/authentication",
                [IdentityControllers.ApplicationUser] = "api/applicationuser",
                [IdentityControllers.ContactUs] = "api/contactus"
            };
        }
    }
}
