using FluentValidation.TestHelper;
using MealPlanner.Api.Features.ShoppingList.Commands.UpdateProductCollected;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.UpdateProductCollected
{
    [TestFixture]
    public class UpdateProductCollectedCommandValidatorTests
    {
        private UpdateProductCollectedCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new UpdateProductCollectedCommandValidator();
        }

        [Test]
        public void ShoppingListId_Empty_HasValidationError()
        {
            var command = new UpdateProductCollectedCommand
            {
                ShoppingListId = Guid.Empty,
                ProductId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ShoppingListId);
        }

        [Test]
        public void ProductId_Empty_HasValidationError()
        {
            var command = new UpdateProductCollectedCommand
            {
                ShoppingListId = Guid.NewGuid(),
                ProductId = Guid.Empty
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }

        [Test]
        public void ValidCommand_HasNoValidationErrors()
        {
            var command = new UpdateProductCollectedCommand
            {
                ShoppingListId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Collected = true
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
