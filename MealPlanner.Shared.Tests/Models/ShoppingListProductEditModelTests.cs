using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListProductEditModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_IsInvalid_BecauseRequiredFieldsMissing()
        {
            // Act
            var model = new ShoppingListProductEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductEditModel.ShoppingListId))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductEditModel.UnitId))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductEditModel.DisplaySequence))), Is.True);
            });
        }

        [Test]
        public void Ctor_SetsProperties_AndValidates_WhenWithinConstraints()
        {
            // Arrange
            var model = new ShoppingListProductEditModel(
                shoppingListId: 1,
                quantity: 2.5m,
                unitId: 3,
                displaySequence: 0);

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
                Assert.That(model.ShoppingListId, Is.EqualTo(1));
                Assert.That(model.Quantity, Is.EqualTo(2.5m));
                Assert.That(model.UnitId, Is.EqualTo(3));
                Assert.That(model.DisplaySequence, Is.EqualTo(0));
            });
        }

        [Test]
        public void Quantity_MustBePositiveOrZero()
        {
            // Arrange: negative quantity
            var model = new ShoppingListProductEditModel
            {
                ShoppingListId = 1,
                UnitId = 2,
                DisplaySequence = 1,
                Quantity = -1m
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductEditModel.Quantity))), Is.True);

            // Arrange: zero quantity allowed by Range(0, int.MaxValue)
            model.Quantity = 0m;

            // Act
            isValid = TryValidate(model, out results);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void UnitId_MustBeAtLeastOne()
        {
            // Arrange
            var model = new ShoppingListProductEditModel
            {
                ShoppingListId = 1,
                Quantity = 1m,
                UnitId = 0,
                DisplaySequence = 1
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductEditModel.UnitId))), Is.True);

            // Arrange valid
            model.UnitId = 1;

            // Act
            isValid = TryValidate(model, out results);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void DisplaySequence_MustBeAtLeastZero()
        {
            // Arrange: negative
            var model = new ShoppingListProductEditModel
            {
                ShoppingListId = 1,
                Quantity = 1m,
                UnitId = 1,
                DisplaySequence = -1
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductEditModel.DisplaySequence))), Is.True);

            // Arrange: zero allowed
            model.DisplaySequence = 0;

            // Act
            isValid = TryValidate(model, out results);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ToString_UsesQuantityUnitAndProduct_WhenAvailable()
        {
            // Arrange
            var model = new ShoppingListProductEditModel
            {
                Quantity = 3.5m,
                Unit = new UnitModel { Name = "kg" },
                Product = new ProductModel { Name = "Apples" }
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("3.5 kg Apples"));
        }

        [Test]
        public void ToString_OmitsNullUnitOrProduct()
        {
            // Arrange: no unit or product
            var model = new ShoppingListProductEditModel
            {
                Quantity = 2m,
                Unit = null,
                Product = null
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("2"));
        }
    }
}