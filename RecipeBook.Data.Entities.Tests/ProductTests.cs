namespace RecipeBook.Data.Entities.Tests
{
    [TestFixture]
    public class ProductTests
    {
        [Test]
        public void DefaultCtor_Sets_Defaults()
        {
            var product = new Product();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(product.Id, Is.EqualTo(Guid.Empty));
                Assert.That(product.Name, Is.Null);
                Assert.That(product.ImageContent, Is.Null);
                Assert.That(product.BaseUnit, Is.Null);
                Assert.That(product.BaseUnitId, Is.EqualTo(Guid.Empty));
                Assert.That(product.ProductCategory, Is.Null);
                Assert.That(product.ProductCategoryId, Is.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            var baseUnit = new Unit { Id = Guid.NewGuid(), Name = "g" };
            var category = new ProductCategory { Id = Guid.NewGuid(), Name = "Flour" };
            var image = new byte[] { 1, 2, 3 };
            var productId = Guid.NewGuid();

            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                ImageContent = image,
                BaseUnit = baseUnit,
                BaseUnitId = baseUnit.Id,
                ProductCategory = category,
                ProductCategoryId = category.Id
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(product.Id, Is.EqualTo(productId));
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
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var baseUnitId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Sugar",
                ProductCategoryId = categoryId,
                BaseUnitId = baseUnitId
            };

            var text = product.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Sugar"));
                Assert.That(text, Does.Contain(productId.ToString()));
                Assert.That(text, Does.Contain(categoryId.ToString()));
                Assert.That(text, Does.Contain(baseUnitId.ToString()));
            }
        }
    }
}
