using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class ProductModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ProductModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(Guid.Empty));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.ImageUrl, Is.Null);
                Assert.That(model.BaseUnit, Is.Null);
                Assert.That(model.ProductCategory, Is.Null);
                Assert.That(model.ProductCategoryName, Is.Null);
                Assert.That(model.ProductCategoryId, Is.Null);

                // BaseModel defaults
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Ctor_SetsIdAndName()
        {
            // Arrange
            var id = Guid.NewGuid();
            const string name = "Flour";

            // Act
            var model = new ProductModel(id, name);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
                Assert.That(model.ToString(), Is.EqualTo(name));
            }
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ProductModel(Guid.NewGuid(), null!);
            });
        }
    }
}