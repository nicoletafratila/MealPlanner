using FluentValidation.TestHelper;
using RecipeBook.Api.Features.RecipeCategory.Commands.Delete;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.Delete
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
            // Arrange
            var command = new DeleteCommand { Id = Guid.Empty };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_NotEmpty_HasNoValidationError()
        {
            // Arrange
            var command = new DeleteCommand { Id = Guid.NewGuid() };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}