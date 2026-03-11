using MealPlanner.Api.Features.ShoppingList.Commands.Delete;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToZero()
        {
            // Act
            var command = new DeleteCommand();

            // Assert
            Assert.That(command.Id, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            const int id = 7;

            // Act
            var command = new DeleteCommand(id);

            // Assert
            Assert.That(command.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id()
        {
            // Arrange
            var command = new DeleteCommand
            {
                // Act
                Id = 42
            };

            // Assert
            Assert.That(command.Id, Is.EqualTo(42));
        }
    }
}