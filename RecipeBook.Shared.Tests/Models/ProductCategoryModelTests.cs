using NUnit.Framework;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class ProductCategoryModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ProductCategoryModel();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(0));
                Assert.That(model.Name, Is.EqualTo(string.Empty));

                // BaseModel defaults
                Assert.That(model.Index, Is.EqualTo(0));
                Assert.That(model.IsSelected, Is.False);
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 7;
            const string name = "Dairy";

            // Act
            var model = new ProductCategoryModel(id, name);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
            });
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ProductCategoryModel(1, null!);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            // Arrange
            var model = new ProductCategoryModel
            {
                Id = 1,
                Name = "Snacks"
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Snacks"));
        }
    }
}