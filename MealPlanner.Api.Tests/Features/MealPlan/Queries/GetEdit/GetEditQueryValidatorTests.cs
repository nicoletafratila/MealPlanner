using FluentValidation.TestHelper;
using MealPlanner.Api.Features.MealPlan.Queries.GetEdit;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryValidatorTests
    {
        private GetEditQueryValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new GetEditQueryValidator();
        }

        [Test]
        public void Id_Zero_HasValidationError()
        {
            var query = new GetEditQuery { Id = 0 };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_Negative_HasValidationError()
        {
            var query = new GetEditQuery { Id = -1 };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_GreaterThanZero_HasNoValidationError()
        {
            var query = new GetEditQuery { Id = 5 };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}