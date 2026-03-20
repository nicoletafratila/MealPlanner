namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class RecipeIngredientTests
    {
        [Test]
        public void ToShoppingListProduct_Maps_Fields_And_Uses_BaseUnit()
        {
            // Arrange
            var baseUnit = new Unit { Id = 2, Name = "gr" };
            var product = new Product
            {
                Id = 10,
                Name = "Flour",
                BaseUnit = baseUnit
            };

            var ingredient = new RecipeIngredient
            {
                Product = product,
                ProductId = product.Id,
                Quantity = 5m,
                Unit = baseUnit,
                UnitId = baseUnit.Id
            };

            const int displaySequence = 7;

            // Act
            var result = ingredient.ToShoppingListProduct(displaySequence);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ProductId, Is.EqualTo(product.Id));
                Assert.That(result.Product, Is.SameAs(product));

                Assert.That(result.Quantity, Is.EqualTo(5m));

                Assert.That(result.UnitId, Is.EqualTo(baseUnit.Id));
                Assert.That(result.Unit, Is.SameAs(baseUnit));

                Assert.That(result.Collected, Is.False);
                Assert.That(result.DisplaySequence, Is.EqualTo(displaySequence));
            }
        }

        [Test]
        public void ToShoppingListProduct_Throws_When_Product_Is_Null()
        {
            var ingredient = new RecipeIngredient
            {
                Product = null,
                Unit = new Unit(),
                Quantity = 1m
            };

            Assert.That(
                () => ingredient.ToShoppingListProduct(1),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Product must be set"));
        }

        [Test]
        public void ToShoppingListProduct_Throws_When_BaseUnit_Is_Null()
        {
            var product = new Product
            {
                Id = 10,
                Name = "Flour",
                BaseUnit = null
            };

            var ingredient = new RecipeIngredient
            {
                Product = product,
                ProductId = product.Id,
                Unit = new Unit(),
                Quantity = 1m
            };

            Assert.That(
                () => ingredient.ToShoppingListProduct(1),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Product.BaseUnit must be set"));
        }

        [Test]
        public void ToShoppingListProduct_Throws_When_Unit_Is_Null()
        {
            var baseUnit = new Unit { Id = 2, Name = "g" };
            var product = new Product
            {
                Id = 10,
                Name = "Flour",
                BaseUnit = baseUnit
            };

            var ingredient = new RecipeIngredient
            {
                Product = product,
                ProductId = product.Id,
                Unit = null,
                Quantity = 1m
            };

            Assert.That(
                () => ingredient.ToShoppingListProduct(1),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Unit must be set"));
        }
    }
}