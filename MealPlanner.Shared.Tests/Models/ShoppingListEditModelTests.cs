using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListEditModelTests
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
            var model = new ShoppingListEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(0));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.ShopId, Is.EqualTo(0));
                Assert.That(model.Products, Is.Not.Null);
                Assert.That(model.Products.Count, Is.EqualTo(0));

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Name))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Products))), Is.True);
            });
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ShoppingListEditModel
            {
                Id = 1,
                Name = "Weekly Groceries",
                ShopId = 10,
                Products = new List<ShoppingListProductEditModel>
                {
                    new() { ShoppingListId = 1, Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
                }
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
            var model = new ShoppingListEditModel
            {
                Id = 1,
                Name = "",
                ShopId = 1,
                Products = new List<ShoppingListProductEditModel>
                {
                    new() { ShoppingListId = 1, Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
                }
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Name))), Is.True);

            // Too long
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Name))), Is.True);

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
                Id = 1,
                Name = "Test",
                ShopId = 1,
                Products = new List<ShoppingListProductEditModel>()
            };

            // Empty list -> invalid (MinimumCountCollection)
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Products))), Is.True);

            // Null list -> invalid (Required)
            model.Products = null!;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShoppingListEditModel.Products))), Is.True);

            // One product -> valid
            model.Products = new List<ShoppingListProductEditModel>
            {
                new() { ShoppingListId = 1, Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
            };

            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 7;
            const string name = "Party List";
            const int shopId = 3;

            // Act
            var model = new ShoppingListEditModel(id, name, shopId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
                Assert.That(model.ShopId, Is.EqualTo(shopId));
            });
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ShoppingListEditModel(1, null!, 1);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var model = new ShoppingListEditModel
            {
                Id = 1,
                Name = "Holiday Shopping",
                ShopId = 2,
                Products = new List<ShoppingListProductEditModel>
                {
                    new() { ShoppingListId = 1, Quantity = 1m, UnitId = 1, DisplaySequence = 0 }
                }
            };

            Assert.That(model.ToString(), Is.EqualTo("Holiday Shopping"));
        }
    }
}