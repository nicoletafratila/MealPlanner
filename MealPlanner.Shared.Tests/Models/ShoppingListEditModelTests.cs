using System.ComponentModel.DataAnnotations;using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListEditModelTests
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
            var model = new ShoppingListEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(Guid.Empty));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.ShopId, Is.EqualTo(Guid.Empty));
                Assert.That(model.Products, Is.Not.Null);
                Assert.That(model.Products, Is.Empty);

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Name))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Products))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Weekly Groceries",
                ShopId = Guid.NewGuid(),
                Products =
                [
                    new() { ShoppingListId = Guid.NewGuid(), Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
                ]
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void Name_Required_And_MaxLength100()
        {
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "",
                ShopId = Guid.NewGuid(),
                Products =
                [
                    new() { ShoppingListId = Guid.NewGuid(), Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
                ]
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Name))), Is.True);
            }

            // Too long
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Name))), Is.True);
            }

            // Valid length
            model.Name = new string('a', 100);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Products_Required_And_MinimumCount1()
        {
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                ShopId = Guid.NewGuid(),
                Products = []
            };

            // Empty list -> invalid (MinimumCountCollection)
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Products))), Is.True);
            }

            // Null list -> invalid (Required)
            model.Products = null!;
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Products))), Is.True);
            }

            // One product -> valid
            model.Products =
            [
                new() { ShoppingListId = Guid.NewGuid(), Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
            ];

            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            const string name = "Party List";
            var shopId = Guid.NewGuid();

            // Act
            var model = new ShoppingListEditModel(id, name, shopId);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
                Assert.That(model.ShopId, Is.EqualTo(shopId));
            }
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ShoppingListEditModel(Guid.NewGuid(), null!, Guid.NewGuid());
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Holiday Shopping",
                ShopId = Guid.NewGuid(),
                Products =
                [
                    new() { ShoppingListId = Guid.NewGuid(), Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
                ]
            };

            Assert.That(model.ToString(), Is.EqualTo("Holiday Shopping"));
        }
    }
}