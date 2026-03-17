using FluentValidation.TestHelper;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.Update
{
    [TestFixture]
    public class UpdateCommandValidatorTests
    {
        private UpdateCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new UpdateCommand { Model = null! };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void UserId_Empty_HasValidationError()
        {
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "",
                    EmailAddress = "user@example.com"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.UserId);
        }

        [Test]
        public void EmailAddress_Empty_HasValidationError()
        {
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "1",
                    EmailAddress = ""
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "1",
                    Username = "user",
                    EmailAddress = "user@example.com"
                }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}