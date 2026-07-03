using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Entities.Tests
{
    [TestFixture]
    public class ShopDisplaySequenceTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            // Act
            var seq = new ShopDisplaySequence();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seq.Value, Is.Zero);
                Assert.That(seq.Shop, Is.Null);
                Assert.That(seq.ShopId, Is.EqualTo(Guid.Empty));
                Assert.That(seq.ProductCategory, Is.Null);
                Assert.That(seq.ProductCategoryId, Is.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var shop = new Shop();
            var category = new ProductCategory();
            var shopId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            // Act
            var seq = new ShopDisplaySequence
            {
                Value = 5,
                Shop = shop,
                ShopId = shopId,
                ProductCategory = category,
                ProductCategoryId = categoryId
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seq.Value, Is.EqualTo(5));
                Assert.That(seq.Shop, Is.SameAs(shop));
                Assert.That(seq.ShopId, Is.EqualTo(shopId));
                Assert.That(seq.ProductCategory, Is.SameAs(category));
                Assert.That(seq.ProductCategoryId, Is.EqualTo(categoryId));
            }
        }

        [Test]
        public void ToString_Contains_Key_Information()
        {
            var shopId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var seq = new ShopDisplaySequence
            {
                ShopId = shopId,
                ProductCategoryId = categoryId,
                Value = 30
            };

            var text = seq.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain(shopId.ToString()));
                Assert.That(text, Does.Contain(categoryId.ToString()));
                Assert.That(text, Does.Contain("30"));
            }
        }
    }
}
