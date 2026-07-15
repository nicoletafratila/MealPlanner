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
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_ButIsInvalid()
        {
            // Act
            var model = new ShopEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(Guid.Empty));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.DisplaySequence, Is.Not.Null);
                Assert.That(model.DisplaySequence, Is.Empty);
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.DisplaySequence))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = Guid.NewGuid(),
                Name = "My Shop",
                DisplaySequence =
                [
                    new() { ShopId = Guid.NewGuid(), Value = 1, ProductCategory = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "Cat1" } }
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
            var model = new ShopEditModel
            {
                Id = Guid.NewGuid(),
                Name = "",
                DisplaySequence =
                [
                    new() { ShopId = Guid.NewGuid(), Value = 1, ProductCategory = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "Cat" } }
                ]
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);
            }

            // Too long
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);
            }

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
                Id = Guid.NewGuid(),
                Name = "Shop",
                DisplaySequence = []
            };

            // Empty list -> invalid
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.DisplaySequence))), Is.True);
            }

            // Null list -> invalid (Required)
            model.DisplaySequence = null!;
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.DisplaySequence))), Is.True);
            }

            // Valid list
            model.DisplaySequence =
            [
                new() { ShopId = Guid.NewGuid(), Value = 1, ProductCategory = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "Cat" } }
            ];

            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_WithCategories_CreatesDisplaySequence()
        {
            // Arrange
            var cat1Id = Guid.NewGuid();
            var cat2Id = Guid.NewGuid();
            var categories = new List<ProductCategoryModel>
            {
                new() { Id = cat1Id, Name = "Cat1" },
                new() { Id = cat2Id, Name = "Cat2" }
            };

            // Act
            var model = new ShopEditModel(categories);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.DisplaySequence, Is.Not.Null);
                Assert.That(model.DisplaySequence, Has.Count.EqualTo(2));

                Assert.That(model.DisplaySequence![0].Index, Is.EqualTo(1));
                Assert.That(model.DisplaySequence[0].Value, Is.EqualTo(1));
                Assert.That(model.DisplaySequence[0].ProductCategory!.Id, Is.EqualTo(cat1Id));

                Assert.That(model.DisplaySequence[1].Index, Is.EqualTo(2));
                Assert.That(model.DisplaySequence[1].Value, Is.EqualTo(2));
                Assert.That(model.DisplaySequence[1].ProductCategory!.Id, Is.EqualTo(cat2Id));
            }
        }

        [Test]
        public void GetDisplaySequence_ReturnsMatchingItem()
        {
            // Arrange
            var cat1 = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "Cat1" };
            var cat2 = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "Cat2" };

            var model = new ShopEditModel
            {
                DisplaySequence =
                [
                    new() { ShopId = Guid.NewGuid(), Value = 1, ProductCategory = cat1 },
                    new() { ShopId = Guid.NewGuid(), Value = 2, ProductCategory = cat2 },
                ]
            };

            // Act
            var seq1 = model.GetDisplaySequence(cat1.Id);
            var seq2 = model.GetDisplaySequence(cat2.Id);
            var seqNone = model.GetDisplaySequence(Guid.NewGuid());
            var seqNull = model.GetDisplaySequence(null);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seq1, Is.Not.Null);
                Assert.That(seq1!.ProductCategory!.Id, Is.EqualTo(cat1.Id));

                Assert.That(seq2, Is.Not.Null);
                Assert.That(seq2!.ProductCategory!.Id, Is.EqualTo(cat2.Id));

                Assert.That(seqNone, Is.Null);
                Assert.That(seqNull, Is.Null);
            }
        }

        [Test]
        public void Name_Null_IsInvalid_OnValidation()
        {
            // Arrange
            var model = new ShopEditModel
            {
                Id = Guid.NewGuid(),
                Name = null!,
                DisplaySequence =
                [
                    new() { ShopId = Guid.NewGuid(), Value = 1, ProductCategory = new ProductCategoryModel { Id = Guid.NewGuid(), Name = "Cat" } }
                ]
            };

            // Act
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ShopEditModel.Name))), Is.True);
            }
        }
    }
}