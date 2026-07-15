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
        public void DefaultCtor_IsInvalid_WhenQuantityIsNegative()
        {
            // Act
            var model = new RecipeIngredientEditModel
            {
                RecipeId = Guid.NewGuid(),
                Quantity = -1m
            };
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeIngredientEditModel.Quantity))), Is.True);
            }
        }

        [Test]
        public void Ctor_SetsProperties_AndValidates_WhenWithinConstraints()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var model = new RecipeIngredientEditModel(recipeId: recipeId, quantity: 2.5m, unitId: unitId);

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
                Assert.That(model.RecipeId, Is.EqualTo(recipeId));
                Assert.That(model.Quantity, Is.EqualTo(2.5m));
                Assert.That(model.UnitId, Is.EqualTo(unitId));
            }
        }

        [Test]
        public void Quantity_MustBePositive_OrZero()
        {
            // Arrange: negative quantity
            var model = new RecipeIngredientEditModel
            {
                RecipeId = Guid.NewGuid(),
                UnitId = Guid.NewGuid(),
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
