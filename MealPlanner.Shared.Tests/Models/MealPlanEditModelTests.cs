using System.ComponentModel.DataAnnotations;
using MealPlanner.Shared.Models;
using NUnit.Framework;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class MealPlanEditModelTests
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
            var model = new MealPlanEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(0));
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.Recipes, Is.Not.Null);
                Assert.That(model.Recipes.Count, Is.EqualTo(0));

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(MealPlanEditModel.Name))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(MealPlanEditModel.Recipes))), Is.True);
            });
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new MealPlanEditModel
            {
                Id = 1,
                Name = "Weekly Plan",
                Recipes = new List<RecipeModel>
                {
                    new() { Id = 10, Name = "Recipe1" }
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
            var model = new MealPlanEditModel
            {
                Id = 1,
                Name = "",
                Recipes = new List<RecipeModel> { new() { Id = 1, Name = "R1" } }
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(MealPlanEditModel.Name))), Is.True);

            // Too long
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(MealPlanEditModel.Name))), Is.True);

            // Valid length
            model.Name = new string('a', 100);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Recipes_Required_And_MinimumCount1()
        {
            var model = new MealPlanEditModel
            {
                Id = 1,
                Name = "Test",
                Recipes = new List<RecipeModel>()
            };

            // Empty list -> invalid
            var isValid = TryValidate(model, out var results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(MealPlanEditModel.Recipes))), Is.True);

            // Null list -> invalid
            model.Recipes = null!;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(MealPlanEditModel.Recipes))), Is.True);

            // One recipe -> valid
            model.Recipes = new List<RecipeModel> { new() { Id = 1, Name = "R1" } };
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 5;
            const string name = "Plan";

            // Act
            var model = new MealPlanEditModel(id, name);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
            });
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new MealPlanEditModel(1, null!);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var model = new MealPlanEditModel
            {
                Id = 1,
                Name = "My Plan",
                Recipes = new List<RecipeModel> { new() { Id = 1, Name = "R1" } }
            };

            Assert.That(model.ToString(), Is.EqualTo("My Plan"));
        }
    }
}