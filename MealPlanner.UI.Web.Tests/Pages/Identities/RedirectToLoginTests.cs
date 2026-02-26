using System.Security.Claims;
using Bunit;
using MealPlanner.UI.Web.Pages.Identities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.UI.Web.Tests.Pages.Identities
{
    [TestFixture]
    public class RedirectToLoginTests
    {
        private BunitContext _ctx = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<RedirectToLogin> RenderWithAuthState(
            AuthenticationState authState,
            string initialUri)
        {
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            nav.NavigateTo(initialUri);

            // Provide AuthenticationState as a cascading value
            var wrapper = _ctx.Render<CascadingValue<Task<AuthenticationState>>>(ps =>
            {
                ps.Add(p => p.Value, Task.FromResult(authState));
                ps.AddChildContent<RedirectToLogin>();
            });

            return wrapper.FindComponent<RedirectToLogin>();
        }

        // ---------- Redirect behavior ----------

        [Test]
        public void Unauthenticated_AtRoot_RedirectsToLoginWithoutReturnUrl()
        {
            // Arrange: unauthenticated user at root
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var state = new AuthenticationState(anonymous);

            // Act
            var cut = RenderWithAuthState(state, "https://localhost/");

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            // Assert
            Assert.That(nav.Uri, Is.EqualTo("https://localhost/identities/login"));
        }

        [Test]
        public void Unauthenticated_WithPath_RedirectsToLoginWithReturnUrl()
        {
            // Arrange: unauthenticated user at /mealplans/mealplansoverview
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var state = new AuthenticationState(anonymous);

            // Act
            var cut = RenderWithAuthState(
                state,
                "https://localhost/mealplans/mealplansoverview");

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            // Assert
            // ToBaseRelativePath("https://localhost/mealplans/mealplansoverview")
            // -> "mealplans/mealplansoverview"
            Assert.That(
                nav.Uri,
                Is.EqualTo("https://localhost/identities/login?returnUrl=mealplans%2Fmealplansoverview"));
        }

        [Test]
        public void Authenticated_User_DoesNotRedirect()
        {
            // Arrange: authenticated user
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], authenticationType: "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(principal);

            // Act
            var cut = RenderWithAuthState(
                state,
                "https://localhost/mealplans/mealplansoverview");

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            // Assert: URL unchanged
            Assert.That(
                nav.Uri,
                Is.EqualTo("https://localhost/mealplans/mealplansoverview"));
        }
    }
}