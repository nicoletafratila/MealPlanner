using FluentValidation.TestHelper;
using MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.MakeShoppingList
{
    [TestFixture]
    public class MakeShoppingListCommandValidatorTests
    {
        private MakeShoppingListCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new MakeShoppingListCommandValidator();
        }

        [Test]
        public void MealPlanId_Empty_HasValidationError()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = Guid.Empty,
                ShopId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.MealPlanId);
        }

        [Test]
        public void ShopId_Empty_HasValidationError()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = Guid.NewGuid(),
                ShopId = Guid.Empty
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void BothIds_Valid_HasNoValidationErrors()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = Guid.NewGuid(),
                ShopId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MealPlanId);
            result.ShouldNotHaveValidationErrorFor(x => x.ShopId);
        }
    }
}