using System.ComponentModel.DataAnnotations;
using Common.Constants.Units;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class UnitEditModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_HasDefaultValues_AndIsInvalid()
        {
            // Act
            var model = new UnitEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert: Id=0, Name="", UnitType=default, but Required/Range will fail
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.Zero);
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.UnitType, Is.Default);
                Assert.That(isValid, Is.False);
                Assert.That(results, Is.Not.Empty);
            }
        }

        [Test]
        public void Ctor_SetsProperties_AndValidates_WhenWithinConstraints()
        {
            // Arrange
            var model = new UnitEditModel(1, "Kilogram", UnitType.Weight);

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True, "Model should be valid.");
                Assert.That(results, Is.Empty);
                Assert.That(model.Id, Is.EqualTo(1));
                Assert.That(model.Name, Is.EqualTo("Kilogram"));
                Assert.That(model.UnitType, Is.EqualTo(UnitType.Weight));
            }
        }

        [Test]
        public void Name_Required_And_MaxLength100()
        {
            // Arrange: missing name
            var model = new UnitEditModel
            {
                Id = 1,
                Name = "",
                UnitType = UnitType.Weight
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert: invalid because Name is empty (Required)
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(UnitEditModel.Name))), Is.True);
            }

            // Arrange: too long name (>100 chars)
            model.Name = new string('a', 101);

            // Act
            isValid = TryValidate(model, out results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(UnitEditModel.Name))), Is.True);
            }
        }

        [Test]
        public void UnitType_MustBeWithinRange_0_To_3()
        {
            // Arrange: valid UnitType (assuming enum underlying values 0..3)
            var model = new UnitEditModel
            {
                Id = 1,
                Name = "Unit",
                UnitType = (UnitType)2
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.True, "UnitType within 0..3 should be valid.");
                Assert.That(results, Is.Empty);
            }

            // Arrange: invalid UnitType beyond range
            model.UnitType = (UnitType)5;

            // Act
            isValid = TryValidate(model, out results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(UnitEditModel.UnitType))), Is.True);
            }
        }

        [Test]
        public void ToString_ReturnsNameAndType()
        {
            // Arrange
            var model = new UnitEditModel
            {
                Id = 1,
                Name = "Liter",
                UnitType = UnitType.Volume
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Liter (Volume)"));
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new UnitEditModel(1, null!, UnitType.Weight);
            });
        }
    }
}