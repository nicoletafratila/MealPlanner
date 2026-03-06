using RecipeBook.Api.Features.ProductCategory.Commands.Update;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Commands.Update
{
    [TestFixture]
    public class UpdateCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            // Act
            var command = new UpdateCommand();

            // Assert
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            // Arrange
            var model = new ProductCategoryEditModel
            {
                Id = 1,
                Name = "Category1"
            };

            // Act
            var command = new UpdateCommand(model);

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new UpdateCommand(null!);
            });
        }

        [Test]
        public void Can_Set_And_Get_Model_Property()
        {
            // Arrange
            var command = new UpdateCommand();
            var model = new ProductCategoryEditModel
            {
                Id = 2,
                Name = "Category2"
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}