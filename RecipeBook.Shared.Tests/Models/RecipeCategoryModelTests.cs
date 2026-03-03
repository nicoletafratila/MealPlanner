using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class RecipeCategoryModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new RecipeCategoryModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.Zero);
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.DisplaySequence, Is.Zero);

                // From BaseModel
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 3;
            const string name = "Desert";
            const int sequence = 7;

            // Act
            var model = new RecipeCategoryModel(id, name, sequence);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
                Assert.That(model.DisplaySequence, Is.EqualTo(sequence));
            }
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new RecipeCategoryModel(1, null!, 1);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            // Arrange
            var model = new RecipeCategoryModel
            {
                Id = 1,
                Name = "Fel principal",
                DisplaySequence = 3
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Fel principal"));
        }
    }
}