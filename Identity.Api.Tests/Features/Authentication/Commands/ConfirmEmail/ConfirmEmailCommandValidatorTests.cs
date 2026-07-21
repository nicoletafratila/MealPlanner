using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.ConfirmEmail;

namespace Identity.Api.Tests.Features.Authentication.Commands.ConfirmEmail
{
    [TestFixture]
    public class ConfirmEmailCommandValidatorTests
    {
        private ConfirmEmailCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ConfirmEmailCommandValidator();
        }

        [Test]
        public void UserId_Empty_HasValidationError()
        {
            var command = new ConfirmEmailCommand
            {
                UserId = "",
                Token = "token"
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void Token_Empty_HasValidationError()
        {
            var command = new ConfirmEmailCommand
            {
                UserId = "user-id",
                Token = ""
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Token);
        }

        [Test]
        public void ValidCommand_HasNoValidationErrors()
        {
            var command = new ConfirmEmailCommand
            {
                UserId = "user-id",
                Token = "confirm-token"
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
