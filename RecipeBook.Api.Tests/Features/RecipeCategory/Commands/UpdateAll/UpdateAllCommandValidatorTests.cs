using FluentValidation.TestHelper;
using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.UpdateAll
{
    [TestFixture]
    public class UpdateAllCommandValidatorTests
    {
        private UpdateAllCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateAllCommandValidator();
        }

        [Test]
        public void Models_Null_HasValidationError()
        {
            // Arrange
            var command = new UpdateAllCommand
            {
                Models = null
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Models);
        }

        [Test]
        public void Models_Empty_HasValidationError()
        {
            // Arrange
            var command = new UpdateAllCommand
            {
                Models = []
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Models);
        }

        [Test]
        public void Models_WithItems_HasNoValidationError()
        {
            // Arrange
            var command = new UpdateAllCommand
            {
                Models =
                [
                    new() { Id = 1, Name = "Cat1", DisplaySequence = 1 }
                ]
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Models);
        }
    }
}