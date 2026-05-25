using FluentValidation.TestHelper;
using Identity.Api.Features.ApplicationUser.Commands.Unlock;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.Unlock
{
    [TestFixture]
    public class UnlockCommandValidatorTests
    {
        private UnlockCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new UnlockCommandValidator();
        }

        [Test]
        public void UserId_Null_HasValidationError()
        {
            var command = new UnlockCommand { UserId = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void UserId_Empty_HasValidationError()
        {
            var command = new UnlockCommand { UserId = "" };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void UserId_Whitespace_HasValidationError()
        {
            var command = new UnlockCommand { UserId = "   " };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void UserId_Valid_HasNoValidationError()
        {
            var command = new UnlockCommand { UserId = "user-123" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }
    }
}
