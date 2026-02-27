using System.ComponentModel.DataAnnotations;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class RecipeCategoryEditModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_ButIsInvalid()
        {
            // Act
            var model = new RecipeCategoryEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(0));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.DisplaySequence, Is.EqualTo(0));

                Assert.That(isValid, Is.False);
                // Name is empty => Required/StringLength should fail
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeCategoryEditModel.Name))), Is.True);
            });
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new RecipeCategoryEditModel
            {
                Id = 1,
                Name = "Breakfast",
                DisplaySequence = 2
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void Name_Required_And_MaxLength100()
        {
            var model = new RecipeCategoryEditModel
            {
                Id = 1,
                Name = "",
                DisplaySequence = 0
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeCategoryEditModel.Name))), Is.True);

            // Too long name
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeCategoryEditModel.Name))), Is.True);

            // Valid length
            model.Name = new string('a', 100);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void DisplaySequence_MustBeAtLeastZero()
        {
            // Arrange: negative value
            var model = new RecipeCategoryEditModel
            {
                Id = 1,
                Name = "Test",
                DisplaySequence = -1
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeCategoryEditModel.DisplaySequence))), Is.True);

            // Arrange: zero is allowed
            model.DisplaySequence = 0;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 10;
            const string name = "Dessert";
            const int seq = 5;

            // Act
            var model = new RecipeCategoryEditModel(id, name, seq);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
                Assert.That(model.DisplaySequence, Is.EqualTo(seq));
            });
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new RecipeCategoryEditModel(1, null!, 0);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var model = new RecipeCategoryEditModel
            {
                Id = 1,
                Name = "Main Course",
                DisplaySequence = 3
            };

            Assert.That(model.ToString(), Is.EqualTo("Main Course"));
        }
    }
}