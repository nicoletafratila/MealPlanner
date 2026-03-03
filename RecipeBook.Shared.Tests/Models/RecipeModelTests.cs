using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class RecipeModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new RecipeModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.Zero);
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.ImageUrl, Is.Null);
                Assert.That(model.Source, Is.Null);
                Assert.That(model.RecipeCategory, Is.Null);
                Assert.That(model.RecipeCategoryName, Is.Null);
                Assert.That(model.RecipeCategoryId, Is.Null);
                Assert.That(model.EffectiveCategoryName, Is.EqualTo(string.Empty));

                // From BaseModel
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Ctor_SetsIdAndName()
        {
            // Arrange
            const int id = 5;
            const string name = "Lasagna";

            // Act
            var model = new RecipeModel(id, name);

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
                _ = new RecipeModel(1, null!);
            });
        }

        [Test]
        public void EffectiveCategoryName_UsesRecipeCategoryName_WhenAvailable()
        {
            // Arrange
            var model = new RecipeModel
            {
                RecipeCategory = new RecipeCategoryModel { Name = "Main" },
                RecipeCategoryName = "MainCached"
            };

            // Act
            var effective = model.EffectiveCategoryName;

            // Assert: prefers RecipeCategory.Name
            Assert.That(effective, Is.EqualTo("Main"));
        }

        [Test]
        public void EffectiveCategoryName_FallsBackToRecipeCategoryName_WhenNavigationNull()
        {
            // Arrange
            var model = new RecipeModel
            {
                RecipeCategory = null,
                RecipeCategoryName = "FallbackName"
            };

            // Act
            var effective = model.EffectiveCategoryName;

            // Assert
            Assert.That(effective, Is.EqualTo("FallbackName"));
        }

        [Test]
        public void EffectiveCategoryName_ReturnsEmpty_WhenNoCategoryInfo()
        {
            // Arrange
            var model = new RecipeModel
            {
                RecipeCategory = null,
                RecipeCategoryName = null
            };

            // Act
            var effective = model.EffectiveCategoryName;

            // Assert
            Assert.That(effective, Is.EqualTo(string.Empty));
        }
    }
}