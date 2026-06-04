using FluentValidation.TestHelper;
using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.SearchByRecipeId
{
    [TestFixture]
    public class SearchByRecipeIdValidatorTests
    {
        private SearchByRecipeIdValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new SearchByRecipeIdValidator();
        }

        [Test]
        public void RecipeId_Empty_HasValidationError()
        {
            var query = new SearchByRecipeIdQuery { RecipeId = Guid.Empty };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.RecipeId);
        }

        [Test]
        public void RecipeId_ValidGuid_HasNoValidationError()
        {
            var query = new SearchByRecipeIdQuery { RecipeId = Guid.NewGuid() };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.RecipeId);
        }
    }
}