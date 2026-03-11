using MealPlanner.Api.Features.Shop.Commands.Add;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.Add
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
            var model = new ShopEditModel
            {
                Id = 0,
                Name = "NewShop"
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
        public void Can_Set_And_Get_Model()
        {
            // Arrange
            var command = new AddCommand();
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "Shop1"
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}