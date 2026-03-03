using System.ComponentModel.DataAnnotations;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class ProductEditModelTests
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
            var model = new ProductEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.Zero);
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.ImageContent, Is.Null);
                Assert.That(model.ImageUrl, Is.Null);
                Assert.That(model.BaseUnitId, Is.Zero);
                Assert.That(model.ProductCategoryId, Is.Zero);

                Assert.That(isValid, Is.False);
                Assert.That(results, Is.Not.Empty);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "Milk",
                ImageContent = new byte[10],
                BaseUnitId = 2,
                ProductCategoryId = 3
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
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "",
                ImageContent = new byte[10],
                BaseUnitId = 1,
                ProductCategoryId = 1
            };

            // Empty name -> invalid
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductEditModel.Name))), Is.True);
            }

            // Too long name
            model.Name = new string('a', 101);
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductEditModel.Name))), Is.True);
            }

            // Valid length
            model.Name = new string('a', 100);
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ImageContent_Required_And_MaxLength512000()
        {
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = null,
                BaseUnitId = 1,
                ProductCategoryId = 1
            };

            // Null -> invalid (Required)
            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductEditModel.ImageContent))), Is.True);
            }

            // Too large
            model.ImageContent = new byte[512001];
            isValid = TryValidate(model, out results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductEditModel.ImageContent))), Is.True);
            }

            // At limit
            model.ImageContent = new byte[512000];
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void BaseUnitId_MustBeAtLeast1()
        {
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = new byte[10],
                BaseUnitId = 0,
                ProductCategoryId = 1
            };

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductEditModel.BaseUnitId))), Is.True);
            }

            model.BaseUnitId = 1;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ProductCategoryId_MustBeAtLeast1()
        {
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "Test",
                ImageContent = new byte[10],
                BaseUnitId = 1,
                ProductCategoryId = 0
            };

            var isValid = TryValidate(model, out var results);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ProductEditModel.ProductCategoryId))), Is.True);
            }

            model.ProductCategoryId = 1;
            isValid = TryValidate(model, out results);
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 5;
            const string name = "Cheese";
            const int baseUnitId = 2;
            const int productCategoryId = 3;

            // Act
            var model = new ProductEditModel(id, name, baseUnitId, productCategoryId);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
                Assert.That(model.BaseUnitId, Is.EqualTo(baseUnitId));
                Assert.That(model.ProductCategoryId, Is.EqualTo(productCategoryId));
            }
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new ProductEditModel(1, null!, 1, 1);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "Bread",
                ImageContent = new byte[1],
                BaseUnitId = 1,
                ProductCategoryId = 1
            };

            Assert.That(model.ToString(), Is.EqualTo("Bread"));
        }
    }
}