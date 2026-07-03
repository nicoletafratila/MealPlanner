namespace MealPlanner.Data.Entities.Tests
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
                Assert.That(shop.Id, Is.EqualTo(Guid.Empty));
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

            var result = shop.GetDisplaySequence(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetDisplaySequence_Returns_Null_When_CategoryId_Is_Null()
        {
            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { ProductCategoryId = Guid.NewGuid(), Value = 10 }
                ]
            };

            var result = shop.GetDisplaySequence(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetDisplaySequence_Returns_Matching_Sequence()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var match = new ShopDisplaySequence { ProductCategoryId = categoryId, Value = 20 };

            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { ProductCategoryId = Guid.NewGuid(), Value = 10 },
                    match,
                    new() { ProductCategoryId = Guid.NewGuid(), Value = 30 }
                ]
            };

            // Act
            var result = shop.GetDisplaySequence(categoryId);

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
                    new() { ProductCategoryId = Guid.NewGuid(), Value = 10 },
                    new() { ProductCategoryId = Guid.NewGuid(), Value = 20 }
                ]
            };

            var result = shop.GetDisplaySequence(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }
    }
}
