namespace Common.Data.Entities.Tests
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
                Assert.That(seq.ShopId, Is.Zero);
                Assert.That(seq.ProductCategory, Is.Null);
                Assert.That(seq.ProductCategoryId, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var shop = new Shop();
            var category = new ProductCategory();

            // Act
            var seq = new ShopDisplaySequence
            {
                Value = 5,
                Shop = shop,
                ShopId = 1,
                ProductCategory = category,
                ProductCategoryId = 2
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seq.Value, Is.EqualTo(5));
                Assert.That(seq.Shop, Is.SameAs(shop));
                Assert.That(seq.ShopId, Is.EqualTo(1));
                Assert.That(seq.ProductCategory, Is.SameAs(category));
                Assert.That(seq.ProductCategoryId, Is.EqualTo(2));
            }
        }

        [Test]
        public void ToString_Contains_Key_Information()
        {
            var seq = new ShopDisplaySequence
            {
                ShopId = 10,
                ProductCategoryId = 20,
                Value = 30
            };

            var text = seq.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("10"));
                Assert.That(text, Does.Contain("20"));
                Assert.That(text, Does.Contain("30"));
            }
        }
    }
}