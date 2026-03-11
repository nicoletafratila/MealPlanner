using FluentValidation.TestHelper;
using MealPlanner.Api.Features.Shop.Commands.Add;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.Add
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
                Model = new ShopEditModel
                {
                    Id = 0,
                    Name = "NewShop"
                }
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }
    }
}