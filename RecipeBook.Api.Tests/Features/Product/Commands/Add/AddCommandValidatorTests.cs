using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Product.Commands.Add;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Add
{
    [TestFixture]
    public class AddCommandValidatorTests
    {
        private AddCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new AddCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            // Arrange
            var command = new AddCommand
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
            var command = new AddCommand
            {
                Model = new ProductEditModel
                {
                    Id = 0,
                    Name = "NewProduct",
                    BaseUnitId = 1,
                    ProductCategoryId = 2
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }
    }
}