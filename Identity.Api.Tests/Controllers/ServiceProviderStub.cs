using Microsoft.AspNetCore.Authentication;

namespace Identity.Api.Tests.Controllers
{
    /// <summary>
    /// Minimal IServiceProvider stub to return IAuthenticationService.
    /// </summary>
    public sealed class ServiceProviderStub : IServiceProvider
    {
        private readonly IAuthenticationService _authService;

        public ServiceProviderStub(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IAuthenticationService))
                return _authService;

            return null;
        }
    }
}