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
                Assert.That(model.ShopId, Is.EqualTo(Guid.Empty));

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
            var shopId = Guid.NewGuid();

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