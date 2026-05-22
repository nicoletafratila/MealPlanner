using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.ChangePassword;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.ChangePassword
{
    [TestFixture]
    public class ChangePasswordCommandValidatorTests
    {
        private ChangePasswordCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ChangePasswordCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new ChangePasswordCommand { Model = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void UserId_Empty_HasValidationError()
        {
            var command = new ChangePasswordCommand
            {
                Model = new ChangePasswordModel
                {
                    UserId = "",
                    CurrentPassword = "OldPass123!",
                    NewPassword = "NewPass123!",
                    ConfirmPassword = "NewPass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.UserId);
        }

        [Test]
        public void CurrentPassword_Empty_HasValidationError()
        {
            var command = new ChangePasswordCommand
            {
                Model = new ChangePasswordModel
                {
                    UserId = "user-id",
                    CurrentPassword = "",
                    NewPassword = "NewPass123!",
                    ConfirmPassword = "NewPass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.CurrentPassword);
        }

        [Test]
        public void NewPassword_Empty_HasValidationError()
        {
            var command = new ChangePasswordCommand
            {
                Model = new ChangePasswordModel
                {
                    UserId = "user-id",
                    CurrentPassword = "OldPass123!",
                    NewPassword = "",
                    ConfirmPassword = "NewPass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.NewPassword);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new ChangePasswordCommand
            {
                Model = new ChangePasswordModel
                {
                    UserId = "user-id",
                    CurrentPassword = "OldPass123!",
                    NewPassword = "NewPass123!",
                    ConfirmPassword = "NewPass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
