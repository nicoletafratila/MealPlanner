using MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.GetShoppingListProducts
{
    [TestFixture]
    public class GetShoppingListProductsQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdsToZero()
        {
            // Act
            var query = new GetShoppingListProductsQuery();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(query.MealPlanId, Is.EqualTo(0));
                Assert.That(query.ShopId, Is.EqualTo(0));
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int mealPlanId = 5;
            const int shopId = 10;

            // Act
            var query = new GetShoppingListProductsQuery(mealPlanId, shopId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(query.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(query.ShopId, Is.EqualTo(shopId));
            });
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                // Act
                MealPlanId = 7,
                ShopId = 3
            };

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(query.MealPlanId, Is.EqualTo(7));
                Assert.That(query.ShopId, Is.EqualTo(3));
            });
        }
    }
}