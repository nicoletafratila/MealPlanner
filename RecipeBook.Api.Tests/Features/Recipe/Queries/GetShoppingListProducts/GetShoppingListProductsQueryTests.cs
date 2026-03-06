using RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetShoppingListProducts
{
    [TestFixture]
    public class GetShoppingListProductsQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesPropertiesToDefaults()
        {
            // Act
            var query = new GetShoppingListProductsQuery();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.RecipeId, Is.Zero);
                Assert.That(query.ShopId, Is.Zero);
                Assert.That(query.AuthToken, Is.Null);
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int recipeId = 5;
            const int shopId = 10;
            const string token = "abc";

            // Act
            var query = new GetShoppingListProductsQuery(recipeId, shopId, token);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.RecipeId, Is.EqualTo(recipeId));
                Assert.That(query.ShopId, Is.EqualTo(shopId));
                Assert.That(query.AuthToken, Is.EqualTo(token));
            }
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                // Act
                RecipeId = 7,
                ShopId = 3,
                AuthToken = "token123"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.RecipeId, Is.EqualTo(7));
                Assert.That(query.ShopId, Is.EqualTo(3));
                Assert.That(query.AuthToken, Is.EqualTo("token123"));
            }
        }
    }
}