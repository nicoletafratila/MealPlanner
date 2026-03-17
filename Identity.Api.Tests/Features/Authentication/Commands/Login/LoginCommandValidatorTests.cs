using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.Login
{
    [TestFixture]
    public class LoginCommandValidatorTests
    {
        private LoginCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new LoginCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new LoginCommand { Model = null! };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void Username_Empty_HasValidationError()
        {
            var command = new LoginCommand
            {
                Model = new LoginModel
                {
                    Username = "",
                    Password = "pwd"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Username);
        }

        [Test]
        public void Password_Empty_HasValidationError()
        {
            var command = new LoginCommand
            {
                Model = new LoginModel
                {
                    Username = "user",
                    Password = ""
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Password);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new LoginCommand
            {
                Model = new LoginModel
                {
                    Username = "user",
                    Password = "pwd"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}