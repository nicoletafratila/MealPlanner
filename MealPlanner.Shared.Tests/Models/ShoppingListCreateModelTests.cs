using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListCreateModelTests
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
            var model = new ShoppingListCreateModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.MealPlanId, Is.EqualTo(Guid.Empty));
                Assert.That(model.ShopId, Is.EqualTo(Guid.Empty));

                // BaseModel defaults
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListCreateModel.MealPlanId))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListCreateModel.ShopId))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ShoppingListCreateModel(Guid.NewGuid(), Guid.NewGuid());

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
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
            var mealPlanId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            // Act
            var model = new ShoppingListCreateModel(mealPlanId, shopId);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(model.ShopId, Is.EqualTo(shopId));
            }
        }
    }
}