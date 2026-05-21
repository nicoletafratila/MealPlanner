using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.Register;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.Register
{
    [TestFixture]
    public class RegisterCommandValidatorTests
    {
        private RegisterCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new RegisterCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new RegisterCommand { Model = null! };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void Username_Empty_HasValidationError()
        {
            var command = new RegisterCommand
            {
                Model = new RegistrationModel
                {
                    Username = "",
                    Password = "Test123!",
                    EmailAddress = "user@example.com"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Username);
        }

        [Test]
        public void Password_Empty_HasValidationError()
        {
            var command = new RegisterCommand
            {
                Model = new RegistrationModel
                {
                    Username = "user",
                    Password = "",
                    EmailAddress = "user@example.com"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Password);
        }

        [Test]
        public void EmailAddress_Empty_HasValidationError()
        {
            var command = new RegisterCommand
            {
                Model = new RegistrationModel
                {
                    Username = "user",
                    Password = "Test123!",
                    EmailAddress = ""
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void EmailAddress_Invalid_HasValidationError()
        {
            var command = new RegisterCommand
            {
                Model = new RegistrationModel
                {
                    Username = "user",
                    Password = "Test123!",
                    EmailAddress = "not-an-email"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new RegisterCommand
            {
                Model = new RegistrationModel
                {
                    Username = "user",
                    Password = "Test123!",
                    ConfirmPassword = "Test123!",
                    EmailAddress = "user@example.com"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
