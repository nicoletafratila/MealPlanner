using FluentValidation.TestHelper;
using MealPlanner.Api.Features.Shop.Commands.ShareAll;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.ShareAll
{
    [TestFixture]
    public class ShareAllCommandValidatorTests
    {
        private ShareAllCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ShareAllCommandValidator();
        }

        [Test]
        public void TargetUserId_Empty_HasValidationError()
        {
            var command = new ShareAllCommand { TargetUserId = string.Empty };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.TargetUserId);
        }

        [Test]
        public void ValidCommand_HasNoValidationErrors()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
