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
            Assert.Multiple(() =>
            {
                Assert.That(command.MealPlanId, Is.EqualTo(0));
                Assert.That(command.ShopId, Is.EqualTo(0));
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int mealPlanId = 5;
            const int shopId = 10;

            // Act
            var command = new MakeShoppingListCommand(mealPlanId, shopId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(command.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(command.ShopId, Is.EqualTo(shopId));
            });
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var command = new MakeShoppingListCommand
            {
                // Act
                MealPlanId = 7,
                ShopId = 3
            };

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(command.MealPlanId, Is.EqualTo(7));
                Assert.That(command.ShopId, Is.EqualTo(3));
            });
        }
    }
}