using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.ResetPassword;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.ResetPassword
{
    [TestFixture]
    public class ResetPasswordCommandValidatorTests
    {
        private ResetPasswordCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ResetPasswordCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new ResetPasswordCommand { Model = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void UserId_Empty_HasValidationError()
        {
            var command = new ResetPasswordCommand
            {
                Model = new ResetPasswordModel
                {
                    UserId = "",
                    Token = "token",
                    NewPassword = "Pass123!",
                    ConfirmPassword = "Pass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.UserId);
        }

        [Test]
        public void Token_Empty_HasValidationError()
        {
            var command = new ResetPasswordCommand
            {
                Model = new ResetPasswordModel
                {
                    UserId = "user-id",
                    Token = "",
                    NewPassword = "Pass123!",
                    ConfirmPassword = "Pass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Token);
        }

        [Test]
        public void NewPassword_Empty_HasValidationError()
        {
            var command = new ResetPasswordCommand
            {
                Model = new ResetPasswordModel
                {
                    UserId = "user-id",
                    Token = "token",
                    NewPassword = "",
                    ConfirmPassword = "Pass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.NewPassword);
        }

        [Test]
        public void ConfirmPassword_DoesNotMatchNewPassword_HasValidationError()
        {
            var command = new ResetPasswordCommand
            {
                Model = new ResetPasswordModel
                {
                    UserId = "user-id",
                    Token = "token",
                    NewPassword = "Pass123!",
                    ConfirmPassword = "DifferentPass!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.ConfirmPassword);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new ResetPasswordCommand
            {
                Model = new ResetPasswordModel
                {
                    UserId = "user-id",
                    Token = "reset-token",
                    NewPassword = "Pass123!",
                    ConfirmPassword = "Pass123!"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
