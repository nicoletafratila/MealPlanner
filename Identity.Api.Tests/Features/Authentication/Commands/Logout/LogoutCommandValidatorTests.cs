using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.Logout;

namespace Identity.Api.Tests.Features.Authentication.Commands.Logout
{
    [TestFixture]
    public class LogoutCommandValidatorTests
    {
        private LogoutCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new LogoutCommandValidator();
        }

        [Test]
        public void LogoutCommand_HasNoValidationErrors()
        {
            // Arrange
            var command = new LogoutCommand();

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}