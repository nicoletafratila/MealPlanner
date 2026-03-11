using FluentValidation.TestHelper;
using MealPlanner.Api.Features.ShoppingList.Commands.Delete;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Commands.Delete
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
            var command = new DeleteCommand { Id = 0 };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_Negative_HasValidationError()
        {
            var command = new DeleteCommand { Id = -1 };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_GreaterThanZero_HasNoValidationError()
        {
            var command = new DeleteCommand { Id = 5 };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}