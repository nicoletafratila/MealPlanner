namespace MealPlanner.Data.Entities.Tests
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
                Assert.That(list.ShopId, Is.EqualTo(Guid.Empty));
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

            var shopId = Guid.NewGuid();

            // Act
            var list = new ShoppingList
            {
                Id = 10,
                Name = "Weekly Groceries",
                Shop = shop,
                ShopId = shopId,
                Products = productList
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(list.Id, Is.EqualTo(10));
                Assert.That(list.Name, Is.EqualTo("Weekly Groceries"));
                Assert.That(list.Shop, Is.SameAs(shop));
                Assert.That(list.ShopId, Is.EqualTo(shopId));
                Assert.That(list.Products, Is.SameAs(productList));
                Assert.That(list.Products, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void ToString_Contains_Name_Id_And_ShopId()
        {
            var shopId = Guid.NewGuid();
            var list = new ShoppingList
            {
                Id = 3,
                Name = "Test List",
                ShopId = shopId
            };

            var text = list.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Test List"));
                Assert.That(text, Does.Contain("3"));
                Assert.That(text, Does.Contain(shopId.ToString()));
            }
        }
    }
}
