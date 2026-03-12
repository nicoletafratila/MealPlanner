using FluentValidation.TestHelper;
using MealPlanner.Api.Features.MealPlan.Commands.Add;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.MealPlan.Commands.Add
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
            var command = new AddCommand { Model = null! };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void Model_NotNull_HasNoValidationError()
        {
            var command = new AddCommand
            {
                Model = new MealPlanEditModel { Id = 0, Name = "Plan1" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }
    }
}