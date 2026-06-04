using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Recipe.Commands.Delete;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.Delete
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
            var command = new DeleteCommand { Id = Guid.Empty };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_ValidGuid_HasNoValidationError()
        {
            var command = new DeleteCommand { Id = Guid.NewGuid() };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}