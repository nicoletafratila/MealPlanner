using MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.MakeShoppingList
{
    [TestFixture]
    public class MakeShoppingListCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesIdsToZero()
        {
            // Act
            var command = new MakeShoppingListCommand();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.MealPlanId, Is.EqualTo(Guid.Empty));
                Assert.That(command.ShopId, Is.EqualTo(0));
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var mealPlanId = Guid.NewGuid();
            const int shopId = 10;

            // Act
            var command = new MakeShoppingListCommand(mealPlanId, shopId);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(command.ShopId, Is.EqualTo(shopId));
            }
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var mealPlanId = Guid.NewGuid();
            var command = new MakeShoppingListCommand
            {
                // Act
                MealPlanId = mealPlanId,
                ShopId = 3
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(command.ShopId, Is.EqualTo(3));
            }
        }
    }
}