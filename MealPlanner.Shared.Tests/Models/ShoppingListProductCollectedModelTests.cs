using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListProductCollectedModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_ButIsInvalid()
        {
            // Act
            var model = new ShoppingListProductCollectedModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.ShoppingListId, Is.EqualTo(Guid.Empty));
                Assert.That(model.ProductId, Is.EqualTo(Guid.Empty));
                Assert.That(model.Collected, Is.False);

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductCollectedModel.ShoppingListId))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductCollectedModel.ProductId))), Is.True);
            }
        }

        [Test]
        public void ShoppingListId_Required_WhenEmpty()
        {
            var model = new ShoppingListProductCollectedModel
            {
                ShoppingListId = Guid.Empty,
                ProductId = Guid.NewGuid()
            };

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductCollectedModel.ShoppingListId))), Is.True);
            }
        }

        [Test]
        public void ProductId_Required_WhenEmpty()
        {
            var model = new ShoppingListProductCollectedModel
            {
                ShoppingListId = Guid.NewGuid(),
                ProductId = Guid.Empty
            };

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListProductCollectedModel.ProductId))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            var model = new ShoppingListProductCollectedModel(Guid.NewGuid(), Guid.NewGuid(), true);

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var shoppingListId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var model = new ShoppingListProductCollectedModel(shoppingListId, productId, true);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.ShoppingListId, Is.EqualTo(shoppingListId));
                Assert.That(model.ProductId, Is.EqualTo(productId));
                Assert.That(model.Collected, Is.True);
            }
        }
    }
}
