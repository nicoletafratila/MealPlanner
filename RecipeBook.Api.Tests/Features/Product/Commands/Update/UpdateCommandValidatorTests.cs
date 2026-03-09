using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Product.Commands.Update;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Update
{
    [TestFixture]
    public class UpdateCommandValidatorTests
    {
        private UpdateCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            // Arrange
            var command = new UpdateCommand
            {
                Model = null!
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void Model_NotNull_HasNoValidationError()
        {
            // Arrange
            var command = new UpdateCommand
            {
                Model = new ProductEditModel
                {
                    Id = 1,
                    Name = "Product1",
                    BaseUnitId = 2,
                    ProductCategoryId = 3
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }
    }
}