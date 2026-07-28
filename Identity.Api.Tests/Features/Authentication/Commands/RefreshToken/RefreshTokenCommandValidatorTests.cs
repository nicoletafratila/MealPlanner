using FluentValidation.TestHelper;
using Identity.Api.Features.Authentication.Commands.RefreshToken;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.RefreshToken
{
    [TestFixture]
    public class RefreshTokenCommandValidatorTests
    {
        private RefreshTokenCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new RefreshTokenCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new RefreshTokenCommand { Model = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void RefreshToken_Empty_HasValidationError()
        {
            var command = new RefreshTokenCommand
            {
                Model = new RefreshTokenModel { RefreshToken = "" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.RefreshToken);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new RefreshTokenCommand
            {
                Model = new RefreshTokenModel { RefreshToken = "some-token" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
