using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.ForgotPassword;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.ForgotPassword
{
    [TestFixture]
    public class ForgotPasswordCommandValidatorTests
    {
        private ForgotPasswordCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ForgotPasswordCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new ForgotPasswordCommand { Model = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void EmailAddress_Empty_HasValidationError()
        {
            var command = new ForgotPasswordCommand
            {
                Model = new ForgotPasswordModel { EmailAddress = "" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void EmailAddress_InvalidFormat_HasValidationError()
        {
            var command = new ForgotPasswordCommand
            {
                Model = new ForgotPasswordModel { EmailAddress = "not-an-email" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new ForgotPasswordCommand
            {
                Model = new ForgotPasswordModel { EmailAddress = "user@example.com" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
