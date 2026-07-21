namespace MealPlanner.Data.Entities.Tests
{
    [TestFixture]
    public class ShoppingListTests
    {
        [Test]
        public void DefaultCtor_Sets_Expected_Defaults()
        {
            var list = new ShoppingList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(list.Id, Is.EqualTo(Guid.Empty));
                Assert.That(list.Name, Is.Null);
                Assert.That(list.Shop, Is.Null);
                Assert.That(list.ShopId, Is.EqualTo(Guid.Empty));
                Assert.That(list.Products, Is.Not.Null);
                Assert.That(list.Products, Is.Empty);
                Assert.That(list.CreatedAt, Is.Null);
                Assert.That(list.UpdatedAt, Is.Null);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            var shop = new Shop();
            var productList = new List<ShoppingListProduct>
            {
                new() { ProductId = Guid.NewGuid() },
                new() { ProductId = Guid.NewGuid() }
            };

            var shopId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var createdAt = DateTime.Now.AddDays(-2);
            var updatedAt = DateTime.Now.AddHours(-1);

            var list = new ShoppingList
            {
                Id = id,
                Name = "Weekly Groceries",
                Shop = shop,
                ShopId = shopId,
                Products = productList,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(list.Id, Is.EqualTo(id));
                Assert.That(list.Name, Is.EqualTo("Weekly Groceries"));
                Assert.That(list.Shop, Is.SameAs(shop));
                Assert.That(list.ShopId, Is.EqualTo(shopId));
                Assert.That(list.Products, Is.SameAs(productList));
                Assert.That(list.Products, Has.Count.EqualTo(2));
                Assert.That(list.CreatedAt, Is.EqualTo(createdAt));
                Assert.That(list.UpdatedAt, Is.EqualTo(updatedAt));
            }
        }

        [Test]
        public void ToString_Contains_Name_Id_And_ShopId()
        {
            var shopId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var list = new ShoppingList { Id = id, Name = "Test List", ShopId = shopId };

            var text = list.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Test List"));
                Assert.That(text, Does.Contain(id.ToString()));
                Assert.That(text, Does.Contain(shopId.ToString()));
            }
        }
    }
}
