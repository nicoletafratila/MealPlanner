using MealPlanner.Data.Entities;
using RecipeBook.Data.Entities;

namespace Common.Services.Tests
{
    [TestFixture]
    public class RecipeIngredientExtensionsTests
    {
        [Test]
        public void ToShoppingListProduct_Maps_Fields_And_Uses_BaseUnit()
        {
            var baseUnit = new Unit { Id = Guid.NewGuid(), Name = "gr" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Flour", BaseUnit = baseUnit };
            var ingredient = new RecipeIngredient
            {
                Product = product,
                ProductId = product.Id,
                Quantity = 5m,
                Unit = baseUnit,
                UnitId = baseUnit.Id
            };

            var result = ingredient.ToShoppingListProduct(7);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ProductId, Is.EqualTo(product.Id));
                Assert.That(result.Product, Is.SameAs(product));
                Assert.That(result.Quantity, Is.EqualTo(5m));
                Assert.That(result.UnitId, Is.EqualTo(baseUnit.Id));
                Assert.That(result.Unit, Is.SameAs(baseUnit));
                Assert.That(result.Collected, Is.False);
                Assert.That(result.DisplaySequence, Is.EqualTo(7));
            }
        }

        [Test]
        public void ToShoppingListProduct_Throws_When_Product_Is_Null()
        {
            var ingredient = new RecipeIngredient { Product = null, Unit = new Unit(), Quantity = 1m };

            Assert.That(
                () => ingredient.ToShoppingListProduct(1),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Product must be set"));
        }

        [Test]
        public void ToShoppingListProduct_Throws_When_BaseUnit_Is_Null()
        {
            var ingredient = new RecipeIngredient
            {
                Product = new Product { Id = Guid.NewGuid(), BaseUnit = null },
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
            var baseUnit = new Unit { Id = Guid.NewGuid(), Name = "g" };
            var ingredient = new RecipeIngredient
            {
                Product = new Product { Id = Guid.NewGuid(), BaseUnit = baseUnit },
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
