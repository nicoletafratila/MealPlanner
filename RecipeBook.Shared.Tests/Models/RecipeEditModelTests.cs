using System.ComponentModel.DataAnnotations;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class RecipeEditModelTests
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
            var model = new RecipeEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.Zero);
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.Source, Is.Null);
                Assert.That(model.ImageContent, Is.Null);
                Assert.That(model.ImageUrl, Is.Null);
                Assert.That(model.RecipeCategoryId, Is.Zero);
                Assert.That(model.Ingredients, Is.Not.Null);
                Assert.That(model.Ingredients, Is.Empty);

                Assert.That(isValid, Is.False);
                Assert.That(results, Is.Not.Empty);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Pasta",
                Source = "Cookbook",
                ImageContent = new byte[10],
                RecipeCategoryId = 2,
                Ingredients =
                [
                    new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
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
        public void Name_Required_And_Max100Chars()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "",
                ImageContent = new byte[10],
                RecipeCategoryId = 1,
                Ingredients =
                [
                    new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
                ]
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.Name))), Is.True);
            }

            // Too long name
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.Name))), Is.True);
            }

            // Valid length
            model.Name = new string('a', 100);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Source_Max256Chars()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = new byte[10],
                RecipeCategoryId = 1,
                Ingredients =
                [
                    new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
                ],
                Source = new string('a', 257)
            };

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.Source))), Is.True);
            }

            model.Source = new string('a', 256);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ImageContent_Required_And_MaxLength512000()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = null,
                RecipeCategoryId = 1,
                Ingredients =
                [
                    new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
                ]
            };

            // Null -> invalid (Required)
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.ImageContent))), Is.True);
            }

            // Too large
            model.ImageContent = new byte[512001];
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.ImageContent))), Is.True);
            }

            // At limit
            model.ImageContent = new byte[512000];
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void RecipeCategoryId_MustBeAtLeast1()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = new byte[10],
                RecipeCategoryId = 0, // invalid
                Ingredients =
                [
                    new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
                ]
            };

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.RecipeCategoryId))), Is.True);
            }

            model.RecipeCategoryId = 1;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ingredients_Required_And_MinimumCount1()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = new byte[10],
                RecipeCategoryId = 1,
                Ingredients = []
            };

            // Empty list -> invalid (MinimumCountCollection)
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.Ingredients))), Is.True);
            }

            // Null list -> invalid (Required)
            model.Ingredients = null!;
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RecipeEditModel.Ingredients))), Is.True);
            }

            // One ingredient -> valid
            model.Ingredients =
            [
                new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
            ];

            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "My Recipe",
                ImageContent = new byte[10],
                RecipeCategoryId = 1,
                Ingredients =
                [
                    new() { RecipeId = 1, Quantity = 1m, UnitId = 1 }
                ]
            };

            Assert.That(model.ToString(), Is.EqualTo("My Recipe"));
        }
    }
}