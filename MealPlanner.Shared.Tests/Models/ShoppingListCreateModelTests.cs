using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListCreateModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ShoppingListCreateModel();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.MealPlanId, Is.EqualTo(0));
                Assert.That(model.ShopId, Is.EqualTo(0));

                // BaseModel defaults
                Assert.That(model.Index, Is.EqualTo(0));
                Assert.That(model.IsSelected, Is.False);
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int mealPlanId = 10;
            const int shopId = 3;

            // Act
            var model = new ShoppingListCreateModel(mealPlanId, shopId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(model.ShopId, Is.EqualTo(shopId));
            });
        }
    }
}