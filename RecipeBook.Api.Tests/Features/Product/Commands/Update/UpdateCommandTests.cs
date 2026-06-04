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
                Id = Guid.NewGuid(),
                Name = "Product1",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = Guid.NewGuid()
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
                Id = Guid.NewGuid(),
                Name = "Product2",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = Guid.NewGuid()
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}