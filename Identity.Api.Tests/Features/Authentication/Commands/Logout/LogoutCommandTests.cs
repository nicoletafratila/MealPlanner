using Identity.Api.Features.Authentication.Commands.Logout;

namespace Identity.Api.Tests.Features.Authentication.Commands.Logout
{
    [TestFixture]
    public class LogoutCommandTests
    {
        [Test]
        public void Can_Create_Command()
        {
            // Act
            var command = new LogoutCommand();

            // Assert
            Assert.That(command, Is.Not.Null);
        }
    }
}