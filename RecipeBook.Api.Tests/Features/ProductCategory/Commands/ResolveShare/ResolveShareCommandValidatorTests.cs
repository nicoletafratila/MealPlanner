using FluentValidation.TestHelper;
using RecipeBook.Api.Features.ProductCategory.Commands.ResolveShare;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Commands.ResolveShare
{
    [TestFixture]
    public class ResolveShareCommandValidatorTests
    {
        private ResolveShareCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ResolveShareCommandValidator();
        }

        [Test]
        public void TargetUserId_Empty_HasValidationError()
        {
            var command = new ResolveShareCommand { CategoryIds = [Guid.NewGuid()], TargetUserId = string.Empty };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.TargetUserId);
        }

        [Test]
        public void ValidCommand_HasNoValidationErrors()
        {
            var command = new ResolveShareCommand { CategoryIds = [Guid.NewGuid()], TargetUserId = "user1" };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
