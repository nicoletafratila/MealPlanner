using RecipeBook.Api.Features.ProductCategory.Commands.Add;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Commands.Add
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            // Act
            var command = new AddCommand();

            // Assert
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            // Arrange
            var model = new ProductCategoryEditModel
            {
                Id = 0,
                Name = "Category1"
            };

            // Act
            var command = new AddCommand(model);

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new AddCommand(null!);
            });
        }

        [Test]
        public void Can_Set_And_Get_Model_Property()
        {
            // Arrange
            var command = new AddCommand();
            var model = new ProductCategoryEditModel
            {
                Id = 0,
                Name = "Category2"
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}