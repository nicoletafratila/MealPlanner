using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Entities.Tests
{
    [TestFixture]
    public class ShoppingListProductTests
    {
        [Test]
        public void DefaultCtor_Sets_Expected_Defaults()
        {
            var item = new ShoppingListProduct();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ShoppingList, Is.Null);
                Assert.That(item.Product, Is.Null);
                Assert.That(item.Unit, Is.Null);

                Assert.That(item.ShoppingListId, Is.EqualTo(Guid.Empty));
                Assert.That(item.ProductId, Is.EqualTo(Guid.Empty));
                Assert.That(item.UnitId, Is.EqualTo(Guid.Empty));

                Assert.That(item.Quantity, Is.Zero);
                Assert.That(item.Collected, Is.False);
                Assert.That(item.DisplaySequence, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            var shoppingList = new ShoppingList();
            var product = new Product();
            var unit = new Unit();
            var shoppingListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();

            var item = new ShoppingListProduct
            {
                ShoppingList = shoppingList,
                ShoppingListId = shoppingListId,
                Product = product,
                ProductId = productId,
                Unit = unit,
                UnitId = unitId,
                Quantity = 4.5m,
                Collected = true,
                DisplaySequence = 10
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ShoppingList, Is.SameAs(shoppingList));
                Assert.That(item.ShoppingListId, Is.EqualTo(shoppingListId));

                Assert.That(item.Product, Is.SameAs(product));
                Assert.That(item.ProductId, Is.EqualTo(productId));

                Assert.That(item.Unit, Is.SameAs(unit));
                Assert.That(item.UnitId, Is.EqualTo(unitId));

                Assert.That(item.Quantity, Is.EqualTo(4.5m));
                Assert.That(item.Collected, Is.True);
                Assert.That(item.DisplaySequence, Is.EqualTo(10));
            }
        }

        [Test]
        public void ToString_Contains_Key_Information()
        {
            var shoppingListId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var item = new ShoppingListProduct
            {
                ShoppingListId = shoppingListId,
                ProductId = productId,
                UnitId = unitId,
                Quantity = 4m
            };

            var text = item.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain(shoppingListId.ToString()));
                Assert.That(text, Does.Contain(productId.ToString()));
                Assert.That(text, Does.Contain(unitId.ToString()));
                Assert.That(text, Does.Contain("4"));
            }
        }
    }
}
