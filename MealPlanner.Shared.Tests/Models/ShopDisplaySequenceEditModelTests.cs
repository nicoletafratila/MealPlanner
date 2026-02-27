using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShopDisplaySequenceEditModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_AndIsValid()
        {
            // Act
            var model = new ShopDisplaySequenceEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.ShopId, Is.EqualTo(0));
                Assert.That(model.Value, Is.EqualTo(0));
                Assert.That(model.ProductCategory, Is.Null);

                // BaseModel
                Assert.That(model.Index, Is.EqualTo(0));
                Assert.That(model.IsSelected, Is.False);

                // With current annotations (Range(0..)), default is valid
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var category = new ProductCategoryModel { Id = 10, Name = "Dairy" };

            // Act
            var model = new ShopDisplaySequenceEditModel(5, 2, category);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.ShopId, Is.EqualTo(5));
                Assert.That(model.Value, Is.EqualTo(2));
                Assert.That(model.ProductCategory, Is.SameAs(category));
            });
        }

        [Test]
        public void Value_Negative_IsInvalid()
        {
            // Arrange
            var model = new ShopDisplaySequenceEditModel
            {
                ShopId = 1,
                Value = -1,
                ProductCategory = new ProductCategoryModel { Id = 1, Name = "Cat" }
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopDisplaySequenceEditModel.Value))), Is.True);
            });
        }

        [Test]
        public void Value_Zero_OrPositive_IsValid()
        {
            // Arrange
            var model = new ShopDisplaySequenceEditModel
            {
                ShopId = 1,
                Value = 0,
                ProductCategory = new ProductCategoryModel { Id = 1, Name = "Cat" }
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(results, Is.Empty);

            // Now positive
            model.Value = 5;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ToString_UsesProductCategoryName_WhenAvailable()
        {
            // Arrange
            var model = new ShopDisplaySequenceEditModel
            {
                ShopId = 1,
                Value = 3,
                ProductCategory = new ProductCategoryModel { Id = 1, Name = "Snacks" }
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Snacks"));
        }

        [Test]
        public void ToString_UsesFallback_WhenCategoryNull()
        {
            // Arrange
            var model = new ShopDisplaySequenceEditModel
            {
                ShopId = 1,
                Value = 4,
                ProductCategory = null
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Category 4"));
        }
    }
}