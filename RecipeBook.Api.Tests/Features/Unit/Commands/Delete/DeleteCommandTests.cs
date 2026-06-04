using RecipeBook.Api.Features.Unit.Commands.Delete;

namespace RecipeBook.Api.Tests.Features.Unit.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToEmpty()
        {
            // Act
            var command = new DeleteCommand();

            // Assert
            Assert.That(command.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var command = new DeleteCommand(id);

            // Assert
            Assert.That(command.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id_Property()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteCommand
            {
                // Act
                Id = id
            };

            // Assert
            Assert.That(command.Id, Is.EqualTo(id));
        }
    }
}
