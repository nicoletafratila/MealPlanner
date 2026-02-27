using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShopEditModelTests
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
            var model = new ShopEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(0));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.DisplaySequence, Is.Not.Null);
                Assert.That(model.DisplaySequence.Count, Is.EqualTo(0));

                Assert.That(isValid, Is.False);
                // Name empty => invalid
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);
                // DisplaySequence empty => MinimumCountCollection fails
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.DisplaySequence))), Is.True);
            });
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "My Shop",
                DisplaySequence = new List<ShopDisplaySequenceEditModel>
                {
                    new() { ShopId = 1, Value = 1, ProductCategory = new ProductCategoryModel { Id = 10, Name = "Cat1" } }
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
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "",
                DisplaySequence = new List<ShopDisplaySequenceEditModel>
                {
                    new() { ShopId = 1, Value = 1, ProductCategory = new ProductCategoryModel { Id = 1, Name = "Cat" } }
                }
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);

            // Too long
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);

            // Valid length
            model.Name = new string('a', 100);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void DisplaySequence_Required_And_MinimumCount1()
        {
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "Shop",
                DisplaySequence = new List<ShopDisplaySequenceEditModel>()
            };

            // Empty list -> invalid
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.DisplaySequence))), Is.True);

            // Null list -> invalid (Required)
            model.DisplaySequence = null!;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.DisplaySequence))), Is.True);

            // Valid list
            model.DisplaySequence = new List<ShopDisplaySequenceEditModel>
            {
                new() { ShopId = 1, Value = 1, ProductCategory = new ProductCategoryModel { Id = 1, Name = "Cat" } }
            };

            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_WithCategories_CreatesDisplaySequence()
        {
            // Arrange
            var categories = new List<ProductCategoryModel>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" }
            };

            // Act
            var model = new ShopEditModel(categories);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.DisplaySequence, Is.Not.Null);
                Assert.That(model.DisplaySequence.Count, Is.EqualTo(2));

                Assert.That(model.DisplaySequence[0].Index, Is.EqualTo(1));
                Assert.That(model.DisplaySequence[0].Value, Is.EqualTo(1));
                Assert.That(model.DisplaySequence[0].ProductCategory!.Id, Is.EqualTo(1));

                Assert.That(model.DisplaySequence[1].Index, Is.EqualTo(2));
                Assert.That(model.DisplaySequence[1].Value, Is.EqualTo(2));
                Assert.That(model.DisplaySequence[1].ProductCategory!.Id, Is.EqualTo(2));
            });
        }

        [Test]
        public void GetDisplaySequence_ReturnsMatchingItem()
        {
            // Arrange
            var cat1 = new ProductCategoryModel { Id = 1, Name = "Cat1" };
            var cat2 = new ProductCategoryModel { Id = 2, Name = "Cat2" };

            var model = new ShopEditModel
            {
                DisplaySequence = new List<ShopDisplaySequenceEditModel>
                {
                    new() { ShopId = 1, Value = 1, ProductCategory = cat1 },
                    new() { ShopId = 1, Value = 2, ProductCategory = cat2 },
                }
            };

            // Act
            var seq1 = model.GetDisplaySequence(1);
            var seq2 = model.GetDisplaySequence(2);
            var seqNone = model.GetDisplaySequence(999);
            var seqNull = model.GetDisplaySequence(null);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(seq1, Is.Not.Null);
                Assert.That(seq1!.ProductCategory!.Id, Is.EqualTo(1));

                Assert.That(seq2, Is.Not.Null);
                Assert.That(seq2!.ProductCategory!.Id, Is.EqualTo(2));

                Assert.That(seqNone, Is.Null);
                Assert.That(seqNull, Is.Null);
            });
        }

        [Test]
        public void Name_Null_IsInvalid_OnValidation()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = 1,
                Name = null!,
                DisplaySequence = new List<ShopDisplaySequenceEditModel>
                {
                    new() { ShopId = 1, Value = 1, ProductCategory = new ProductCategoryModel { Id = 1, Name = "Cat" } }
                }
            };

            // Act
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);
        }
    }
}