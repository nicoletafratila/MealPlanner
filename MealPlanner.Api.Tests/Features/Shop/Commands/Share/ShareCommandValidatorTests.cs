using FluentValidation.TestHelper;
using MealPlanner.Api.Features.Shop.Commands.Share;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.Share
{
    [TestFixture]
    public class ShareCommandValidatorTests
    {
        private ShareCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ShareCommandValidator();
        }

        [Test]
        public void ShopId_Empty_HasValidationError()
        {
            var command = new ShareCommand { ShopId = Guid.Empty, TargetUserId = "user2" };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void TargetUserId_Empty_HasValidationError()
        {
            var command = new ShareCommand { ShopId = Guid.NewGuid(), TargetUserId = string.Empty };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.TargetUserId);
        }

        [Test]
        public void ValidCommand_HasNoValidationErrors()
        {
            var command = new ShareCommand { ShopId = Guid.NewGuid(), TargetUserId = "user2" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
