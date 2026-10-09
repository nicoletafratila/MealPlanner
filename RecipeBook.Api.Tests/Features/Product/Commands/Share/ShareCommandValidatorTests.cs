using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Product.Commands.Share;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Share
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
        public void ProductId_Empty_HasValidationError()
        {
            var command = new ShareCommand { ProductId = Guid.Empty, TargetUserId = "user1" };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }

        [Test]
        public void TargetUserId_Empty_HasValidationError()
        {
            var command = new ShareCommand { ProductId = Guid.NewGuid(), TargetUserId = string.Empty };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.TargetUserId);
        }

        [Test]
        public void ValidCommand_HasNoValidationErrors()
        {
            var command = new ShareCommand { ProductId = Guid.NewGuid(), TargetUserId = "user1" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
