using Bunit;
using Bunit.TestDoubles;
using MealPlanner.UI.Web.Shared;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class NavMenuTests
    {
        private BunitAuthorizationContext _authContext;
        private BunitContext _ctx = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();
            _authContext = _ctx.AddAuthorization();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        [Test]
        public void NotAuthorized_ShowsOnlyHomeLink()
        {
            // Arrange
            _authContext.SetNotAuthorized();

            // Act
            var cut = _ctx.Render<NavMenu>();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Markup, Does.Contain("Home"));
                Assert.That(cut.Markup, Does.Not.Contain("Recipes"));
                Assert.That(cut.Markup, Does.Not.Contain("Meal plans"));
                Assert.That(cut.Markup, Does.Not.Contain("Users"));
            }
        }

        [Test]
        public void Authorized_NonAdmin_ShowsStandardLinks_ButNotAdminOnlyLinks()
        {
            // Arrange
            _authContext.SetAuthorized("member1");
            _authContext.SetRoles("member");

            // Act
            var cut = _ctx.Render<NavMenu>();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Markup, Does.Contain("Recipes"));
                Assert.That(cut.Markup, Does.Contain("Meal plans"));
                Assert.That(cut.Markup, Does.Contain("Shopping lists"));
                Assert.That(cut.Markup, Does.Contain("Shops"));
                Assert.That(cut.Markup, Does.Contain("Products"));
                Assert.That(cut.Markup, Does.Contain("Recipe categories"));
                Assert.That(cut.Markup, Does.Contain("Product categories"));
                Assert.That(cut.Markup, Does.Not.Contain("Units"));
                Assert.That(cut.Markup, Does.Not.Contain("Users"));
                Assert.That(cut.Markup, Does.Not.Contain("Audit trail"));
            }
        }

        [Test]
        public void Authorized_Admin_ShowsAdminOnlyLinks()
        {
            // Arrange
            _authContext.SetAuthorized("admin1");
            _authContext.SetRoles("admin");

            // Act
            var cut = _ctx.Render<NavMenu>();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Markup, Does.Contain("Units"));
                Assert.That(cut.Markup, Does.Contain("Users"));
                Assert.That(cut.Markup, Does.Contain("Audit trail"));
            }
        }

        [Test]
        public void Click_TogglesNavMenu_WithoutThrowing()
        {
            // Arrange
            _authContext.SetNotAuthorized();
            var cut = _ctx.Render<NavMenu>();

            // Act / Assert
            Assert.DoesNotThrow(() => cut.Find("div.nav-menu").Click());
        }
    }
}
