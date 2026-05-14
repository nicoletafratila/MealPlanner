using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Common.Api
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public string? UserId =>
            httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
