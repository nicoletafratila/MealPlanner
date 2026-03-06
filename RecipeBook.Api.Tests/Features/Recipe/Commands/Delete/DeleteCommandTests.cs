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
                Assert.That(command.Id, Is.Zero);
                Assert.That(command.AuthToken, Is.Null);
            }
        }

        [Test]
        public void Ctor_SetsId_And_AuthToken()
        {
            // Arrange
            const int id = 7;
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
                Id = 42,
                AuthToken = "abc"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(command.Id, Is.EqualTo(42));
                Assert.That(command.AuthToken, Is.EqualTo("abc"));
            }
        }
    }
}