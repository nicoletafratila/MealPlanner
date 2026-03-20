namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class ShoppingListTests
    {
        [Test]
        public void DefaultCtor_Sets_Expected_Defaults()
        {
            // Act
            var list = new ShoppingList();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(list.Id, Is.Zero);
                Assert.That(list.Name, Is.Null);
                Assert.That(list.Shop, Is.Null);
                Assert.That(list.ShopId, Is.Zero);
                Assert.That(list.Products, Is.Not.Null);
                Assert.That(list.Products, Is.Empty);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var shop = new Shop();
            var productList = new List<ShoppingListProduct>
            {
                new() { ProductId = 1 },
                new() { ProductId = 2 }
            };

            // Act
            var list = new ShoppingList
            {
                Id = 10,
                Name = "Weekly Groceries",
                Shop = shop,
                ShopId = 5,
                Products = productList
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(list.Id, Is.EqualTo(10));
                Assert.That(list.Name, Is.EqualTo("Weekly Groceries"));
                Assert.That(list.Shop, Is.SameAs(shop));
                Assert.That(list.ShopId, Is.EqualTo(5));
                Assert.That(list.Products, Is.SameAs(productList));
                Assert.That(list.Products, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void ToString_Contains_Name_Id_And_ShopId()
        {
            var list = new ShoppingList
            {
                Id = 3,
                Name = "Test List",
                ShopId = 7
            };

            var text = list.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Test List"));
                Assert.That(text, Does.Contain("3"));
                Assert.That(text, Does.Contain("7"));
            }
        }
    }
}