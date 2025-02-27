using System.Security.Claims;
using Common.Data.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api
{
    public sealed class PersistentAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly Task<AuthenticationState> defaultUnauthenticatedTask =
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        private readonly Task<AuthenticationState> authenticationStateTask = defaultUnauthenticatedTask;

        public PersistentAuthenticationStateProvider(PersistentComponentState state, IServiceScopeFactory scopeFactory)
        {
            if (!state.TryTakeFromJson<ApplicationUser>(nameof(ApplicationUser), out var userInfo) || userInfo is null)
            {
                return;
            }
            using (var scope = scopeFactory.CreateScope())
            {
                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var claims = userMgr.GetClaimsAsync(userInfo).GetAwaiter().GetResult();
                authenticationStateTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims))));
            }
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => authenticationStateTask;
    }


    //public static ApplicationUser FromClaimsPrincipal(ClaimsPrincipal principal, string accessToken) =>
    //   new()
    //   {
    //       UserId = GetRequiredClaim(principal, UserIdClaimType),
    //       Name = GetRequiredClaim(principal, NameClaimType),
    //       AccessToken = accessToken,
    //   };

    //public ClaimsPrincipal ToClaimsPrincipal() =>
    //    new(new ClaimsIdentity(
    //        [new(UserIdClaimType, UserId), new(NameClaimType, Name), new(AccessTokenClaimType, AccessToken)],
    //        authenticationType: nameof(UserInfo),
    //        nameType: NameClaimType,
    //        roleType: null));

    //private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
    //    principal.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Could not find required '{claimType}' claim.");
}
