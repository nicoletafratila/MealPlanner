using FluentValidation.TestHelper;
using Identity.Api.Features.ApplicationUser.Commands.ResendConfirmationEmail;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.ResendConfirmationEmail
{
    [TestFixture]
    public class ResendConfirmationEmailCommandValidatorTests
    {
        private ResendConfirmationEmailCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ResendConfirmationEmailCommandValidator();
        }

        [Test]
        public void UserId_Null_HasValidationError()
        {
            var command = new ResendConfirmationEmailCommand { UserId = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void UserId_Empty_HasValidationError()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "" };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void UserId_Whitespace_HasValidationError()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "   " };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void UserId_Valid_HasNoValidationError()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "user-123" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }
    }
}
