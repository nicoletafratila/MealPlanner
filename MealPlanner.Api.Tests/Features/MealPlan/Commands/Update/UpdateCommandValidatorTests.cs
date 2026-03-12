using FluentValidation.TestHelper;
using MealPlanner.Api.Features.MealPlan.Commands.Update;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.MealPlan.Commands.Update
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
            var command = new UpdateCommand { Model = null! };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void Model_NotNull_HasNoValidationError()
        {
            var command = new UpdateCommand
            {
                Model = new MealPlanEditModel { Id = 1, Name = "Plan1" }
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }
    }
}