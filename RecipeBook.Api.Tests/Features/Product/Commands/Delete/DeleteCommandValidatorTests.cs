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
        public void Id_Zero_HasValidationError()
        {
            // Arrange
            var command = new DeleteCommand { Id = 0 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_Negative_HasValidationError()
        {
            // Arrange
            var command = new DeleteCommand { Id = -1 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_GreaterThanZero_HasNoValidationError()
        {
            // Arrange
            var command = new DeleteCommand { Id = 5 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}