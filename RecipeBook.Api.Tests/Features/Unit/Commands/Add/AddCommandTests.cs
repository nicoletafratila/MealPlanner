using RecipeBook.Api.Features.Unit.Commands.Add;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Unit.Commands.Add
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesWithNullModel()
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
            var model = new UnitEditModel
            {
                Id = 0,
                Name = "kg",
                UnitType = Common.Constants.Units.UnitType.Weight
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
            var model = new UnitEditModel
            {
                Id = 0,
                Name = "g",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}