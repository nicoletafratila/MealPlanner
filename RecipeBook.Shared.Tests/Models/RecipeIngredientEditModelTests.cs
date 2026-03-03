using System.ComponentModel.DataAnnotations;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class RecipeIngredientEditModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_IsInvalid_BecauseRequiredFieldsMissing()
        {
            // Act
            var model = new RecipeIngredientEditModel
            {
                RecipeId = -1
            };
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeIngredientEditModel.RecipeId))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeIngredientEditModel.UnitId))), Is.True);
            }
        }

        [Test]
        public void Ctor_SetsProperties_AndValidates_WhenWithinConstraints()
        {
            // Arrange
            var model = new RecipeIngredientEditModel(recipeId: 1, quantity: 2.5m, unitId: 3);

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
                Assert.That(model.RecipeId, Is.EqualTo(1));
                Assert.That(model.Quantity, Is.EqualTo(2.5m));
                Assert.That(model.UnitId, Is.EqualTo(3));
            }
        }

        [Test]
        public void Quantity_MustBePositive_OrZero()
        {
            // Arrange: negative quantity
            var model = new RecipeIngredientEditModel
            {
                RecipeId = 1,
                UnitId = 2,
                Quantity = -1m
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeIngredientEditModel.Quantity))), Is.True);
            }

            // Arrange: zero quantity is allowed by Range(0, int.MaxValue)
            model.Quantity = 0m;

            // Act
            isValid = TryValidate(model, out results);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void UnitId_MustBeAtLeastOne()
        {
            // Arrange: invalid unit = 0
            var model = new RecipeIngredientEditModel
            {
                RecipeId = 1,
                Quantity = 1m,
                UnitId = 0
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeIngredientEditModel.UnitId))), Is.True);
            }

            // Arrange: valid unit id
            model.UnitId = 1;

            // Act
            isValid = TryValidate(model, out results);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ToString_UsesQuantityUnitAndProductName_WhenAvailable()
        {
            // Arrange
            var model = new RecipeIngredientEditModel
            {
                Quantity = 2.5m,
                Unit = new UnitModel { Name = "kg" },
                Product = new ProductModel { Name = "Flour" }
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("2.5 kg Flour"));
        }

        [Test]
        public void ToString_OmitsNullUnitOrProduct()
        {
            // Arrange
            var model = new RecipeIngredientEditModel
            {
                Quantity = 1m,
                Unit = null,
                Product = null
            };

            // Act
            var text = model.ToString();

            // Assert
            // Only quantity since both names are null
            Assert.That(text, Is.EqualTo("1"));
        }
    }
}