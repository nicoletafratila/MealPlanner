using RecipeBook.Api.Features.Recipe.Commands.Delete;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesProperties()
        {
            // Act
            var command = new DeleteCommand();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.Id, Is.EqualTo(Guid.Empty));
                Assert.That(command.AuthToken, Is.Null);
            }
        }

        [Test]
        public void Ctor_SetsId_And_AuthToken()
        {
            // Arrange
            var id = Guid.NewGuid();
            const string token = "token123";

            // Act
            var command = new DeleteCommand(id, token);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.Id, Is.EqualTo(id));
                Assert.That(command.AuthToken, Is.EqualTo(token));
            }
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var command = new DeleteCommand
            {
                // Act
                Id = Guid.NewGuid(),
                AuthToken = "abc"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(command.AuthToken, Is.EqualTo("abc"));
            }
        }
    }
}