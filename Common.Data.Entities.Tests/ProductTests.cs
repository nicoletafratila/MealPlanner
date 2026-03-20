namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class ProductTests
    {
        [Test]
        public void DefaultCtor_Sets_Defaults()
        {
            // Act
            var product = new Product();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(product.Id, Is.Zero);
                Assert.That(product.Name, Is.Null);
                Assert.That(product.ImageContent, Is.Null);

                Assert.That(product.BaseUnit, Is.Null);
                Assert.That(product.BaseUnitId, Is.Zero);

                Assert.That(product.ProductCategory, Is.Null);
                Assert.That(product.ProductCategoryId, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var baseUnit = new Unit { Id = 2, Name = "g" };
            var category = new ProductCategory { Id = 3, Name = "Flour" };
            var image = new byte[] { 1, 2, 3 };

            // Act
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                ImageContent = image,
                BaseUnit = baseUnit,
                BaseUnitId = baseUnit.Id,
                ProductCategory = category,
                ProductCategoryId = category.Id
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(product.Id, Is.EqualTo(1));
                Assert.That(product.Name, Is.EqualTo("Test Product"));
                Assert.That(product.ImageContent, Is.SameAs(image));

                Assert.That(product.BaseUnit, Is.SameAs(baseUnit));
                Assert.That(product.BaseUnitId, Is.EqualTo(baseUnit.Id));

                Assert.That(product.ProductCategory, Is.SameAs(category));
                Assert.That(product.ProductCategoryId, Is.EqualTo(category.Id));
            }
        }

        [Test]
        public void ToString_Contains_Name_Id_CategoryId_And_BaseUnitId()
        {
            var product = new Product
            {
                Id = 5,
                Name = "Sugar",
                ProductCategoryId = 10,
                BaseUnitId = 20
            };

            var text = product.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Sugar"));
                Assert.That(text, Does.Contain("5"));
                Assert.That(text, Does.Contain("10"));
                Assert.That(text, Does.Contain("20"));
            }
        }
    }
}