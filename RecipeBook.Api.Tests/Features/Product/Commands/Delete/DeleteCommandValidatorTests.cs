using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Product.Commands.Delete;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandValidatorTests
    {
        private DeleteCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new DeleteCommandValidator();
        }

        [Test]
        public void Id_Empty_HasValidationError()
        {
            var result = _validator.TestValidate(new DeleteCommand { Id = Guid.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_NotEmpty_HasNoValidationError()
        {
            var result = _validator.TestValidate(new DeleteCommand { Id = Guid.NewGuid() });
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
