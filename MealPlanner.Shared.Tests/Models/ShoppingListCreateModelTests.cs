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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.MealPlanId, Is.EqualTo(Guid.Empty));
                Assert.That(model.ShopId, Is.Zero);

                // BaseModel defaults
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var mealPlanId = Guid.NewGuid();
            const int shopId = 3;

            // Act
            var model = new ShoppingListCreateModel(mealPlanId, shopId);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(model.ShopId, Is.EqualTo(shopId));
            }
        }
    }
}