using Microsoft.AspNetCore.Authentication;

namespace Identity.Api.Tests.Controllers
{
    /// <summary>
    /// Minimal IServiceProvider stub to return IAuthenticationService.
    /// </summary>
    public class ServiceProviderStub(IAuthenticationService authService) : IServiceProvider
    {
        private readonly IAuthenticationService _authService = authService;

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IAuthenticationService))
                return _authService;

            return null;
        }
    }
}