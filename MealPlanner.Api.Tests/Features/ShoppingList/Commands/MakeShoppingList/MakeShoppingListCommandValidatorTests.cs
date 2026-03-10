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
        public void MealPlanId_Zero_HasValidationError()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 0,
                ShopId = 1
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.MealPlanId);
        }

        [Test]
        public void MealPlanId_Negative_HasValidationError()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = -1,
                ShopId = 1
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.MealPlanId);
        }

        [Test]
        public void ShopId_Zero_HasValidationError()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 1,
                ShopId = 0
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void ShopId_Negative_HasValidationError()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 1,
                ShopId = -2
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void BothIds_GreaterThanZero_HasNoValidationErrors()
        {
            var command = new MakeShoppingListCommand
            {
                MealPlanId = 3,
                ShopId = 4
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MealPlanId);
            result.ShouldNotHaveValidationErrorFor(x => x.ShopId);
        }
    }
}