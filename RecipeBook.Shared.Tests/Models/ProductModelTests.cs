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
                Assert.That(model.Id, Is.Zero);
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.ImageUrl, Is.Null);
                Assert.That(model.BaseUnit, Is.Null);
                Assert.That(model.ProductCategory, Is.Null);
                Assert.That(model.ProductCategoryName, Is.Null);
                Assert.That(model.ProductCategoryId, Is.Null);
                Assert.That(model.EffectiveCategoryName, Is.EqualTo(string.Empty));

                // BaseModel defaults
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Ctor_SetsIdAndName()
        {
            // Arrange
            const int id = 10;
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
                _ = new ProductModel(1, null!);
            });
        }

        [Test]
        public void EffectiveCategoryName_UsesProductCategoryName_WhenAvailable()
        {
            // Arrange
            var model = new ProductModel
            {
                ProductCategory = new ProductCategoryModel { Name = "Dairy" },
                ProductCategoryName = "DairyCached"
            };

            // Act
            var effective = model.EffectiveCategoryName;

            // Assert: prefers navigation property
            Assert.That(effective, Is.EqualTo("Dairy"));
        }

        [Test]
        public void EffectiveCategoryName_FallsBackToFlattenedName_WhenNavigationNull()
        {
            // Arrange
            var model = new ProductModel
            {
                ProductCategory = null,
                ProductCategoryName = "Snacks"
            };

            // Act
            var effective = model.EffectiveCategoryName;

            // Assert
            Assert.That(effective, Is.EqualTo("Snacks"));
        }

        [Test]
        public void EffectiveCategoryName_ReturnsEmpty_WhenNoCategoryInfo()
        {
            // Arrange
            var model = new ProductModel
            {
                ProductCategory = null,
                ProductCategoryName = null
            };

            // Act
            var effective = model.EffectiveCategoryName;

            // Assert
            Assert.That(effective, Is.EqualTo(string.Empty));
        }
    }
}