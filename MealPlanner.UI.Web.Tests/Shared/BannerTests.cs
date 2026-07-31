using Bunit;
using MealPlanner.UI.Web.Shared;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class BannerTests
    {
        private BunitContext _ctx = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        [Test]
        public void Renders_BannerImage_WithExpectedSourceAndAlt()
        {
            // Act
            var cut = _ctx.Render<Banner>();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Markup, Does.Contain("banner-image"));
                Assert.That(cut.Markup, Does.Contain(@"Images\banner2.png"));
                Assert.That(cut.Markup, Does.Contain("Banner Image"));
            }
        }

        [Test]
        public void Renders_WrappingDiv_WithBannerClass()
        {
            // Act
            var cut = _ctx.Render<Banner>();

            // Assert
            var div = cut.Find("div.banner");
            Assert.That(div, Is.Not.Null);
        }
    }
}
