namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class ShoppingListProductTests
    {
        [Test]
        public void DefaultCtor_Sets_Expected_Defaults()
        {
            // Act
            var item = new ShoppingListProduct();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ShoppingList, Is.Null);
                Assert.That(item.Product, Is.Null);
                Assert.That(item.Unit, Is.Null);

                Assert.That(item.ShoppingListId, Is.Zero);
                Assert.That(item.ProductId, Is.Zero);
                Assert.That(item.UnitId, Is.Zero);

                Assert.That(item.Quantity, Is.Zero);
                Assert.That(item.Collected, Is.False);
                Assert.That(item.DisplaySequence, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var shoppingList = new ShoppingList();
            var product = new Product();
            var unit = new Unit();

            // Act
            var item = new ShoppingListProduct
            {
                ShoppingList = shoppingList,
                ShoppingListId = 1,
                Product = product,
                ProductId = 2,
                Unit = unit,
                UnitId = 3,
                Quantity = 4.5m,
                Collected = true,
                DisplaySequence = 10
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ShoppingList, Is.SameAs(shoppingList));
                Assert.That(item.ShoppingListId, Is.EqualTo(1));

                Assert.That(item.Product, Is.SameAs(product));
                Assert.That(item.ProductId, Is.EqualTo(2));

                Assert.That(item.Unit, Is.SameAs(unit));
                Assert.That(item.UnitId, Is.EqualTo(3));

                Assert.That(item.Quantity, Is.EqualTo(4.5m));
                Assert.That(item.Collected, Is.True);
                Assert.That(item.DisplaySequence, Is.EqualTo(10));
            }
        }

        [Test]
        public void ToString_Contains_Key_Information()
        {
            var item = new ShoppingListProduct
            {
                ShoppingListId = 1,
                ProductId = 2,
                UnitId = 3,
                Quantity = 4m
            };

            var text = item.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("1"));
                Assert.That(text, Does.Contain("2"));
                Assert.That(text, Does.Contain("3"));
                Assert.That(text, Does.Contain("4"));
            }
        }
    }
}