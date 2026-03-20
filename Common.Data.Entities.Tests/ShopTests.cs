namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class ShopTests
    {
        [Test]
        public void DefaultCtor_Sets_Defaults()
        {
            // Act
            var shop = new Shop();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(shop.Id, Is.Zero);
                Assert.That(shop.Name, Is.Null);
                Assert.That(shop.DisplaySequence, Is.Not.Null);
                Assert.That(shop.DisplaySequence, Is.Empty);
            }
        }

        [Test]
        public void GetDisplaySequence_Returns_Null_When_No_DisplaySequence()
        {
            var shop = new Shop
            {
                DisplaySequence = []
            };

            var result = shop.GetDisplaySequence(1);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetDisplaySequence_Returns_Null_When_CategoryId_Is_Null()
        {
            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { ProductCategoryId = 1, Value = 10 }
                ]
            };

            var result = shop.GetDisplaySequence(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetDisplaySequence_Returns_Matching_Sequence()
        {
            // Arrange
            var match = new ShopDisplaySequence { ProductCategoryId = 2, Value = 20 };

            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { ProductCategoryId = 1, Value = 10 },
                    match,
                    new() { ProductCategoryId = 3, Value = 30 }
                ]
            };

            // Act
            var result = shop.GetDisplaySequence(2);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.SameAs(match));
            }
        }

        [Test]
        public void GetDisplaySequence_Returns_Null_When_No_Category_Match()
        {
            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { ProductCategoryId = 1, Value = 10 },
                    new() { ProductCategoryId = 2, Value = 20 }
                ]
            };

            var result = shop.GetDisplaySequence(99);

            Assert.That(result, Is.Null);
        }
    }
}