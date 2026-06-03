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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.MealPlanId, Is.EqualTo(Guid.Empty));
                Assert.That(query.ShopId, Is.EqualTo(0));
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var mealPlanId = Guid.NewGuid();
            const int shopId = 10;

            // Act
            var query = new GetShoppingListProductsQuery(mealPlanId, shopId);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(query.ShopId, Is.EqualTo(shopId));
            }
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var mealPlanId = Guid.NewGuid();
            var query = new GetShoppingListProductsQuery
            {
                // Act
                MealPlanId = mealPlanId,
                ShopId = 3
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(query.ShopId, Is.EqualTo(3));
            }
        }
    }
}