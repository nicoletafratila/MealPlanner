using RecipeBook.Api.Features.Product.Commands.Update;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Update
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
            var model = new ProductEditModel
            {
                Id = 1,
                Name = "Product1",
                BaseUnitId = 2,
                ProductCategoryId = 3
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
            var model = new ProductEditModel
            {
                Id = 2,
                Name = "Product2",
                BaseUnitId = 5,
                ProductCategoryId = 6
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}