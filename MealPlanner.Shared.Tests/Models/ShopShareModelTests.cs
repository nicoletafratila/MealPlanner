using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShopShareModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ShopShareModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.ShopId, Is.EqualTo(Guid.Empty));
                Assert.That(model.TargetUserId, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public void Properties_CanBeSet()
        {
            // Arrange
            var shopId = Guid.NewGuid();

            // Act
            var model = new ShopShareModel { ShopId = shopId, TargetUserId = "user2" };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.ShopId, Is.EqualTo(shopId));
                Assert.That(model.TargetUserId, Is.EqualTo("user2"));
            }
        }
    }
}
