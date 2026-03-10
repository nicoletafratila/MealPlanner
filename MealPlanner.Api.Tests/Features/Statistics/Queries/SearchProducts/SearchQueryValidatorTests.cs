using FluentValidation.TestHelper;
using MealPlanner.Api.Features.Statistics.Queries.SearchProducts;

namespace MealPlanner.Api.Tests.Features.Statistics.Queries.SearchProducts
{
    [TestFixture]
    public class SearchQueryValidatorTests
    {
        private SearchQueryValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new SearchQueryValidator();
        }

        [Test]
        public void CategoryIds_Null_HasValidationError()
        {
            // Arrange
            var query = new SearchQuery
            {
                CategoryIds = null
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryIds);
        }

        [Test]
        public void CategoryIds_Empty_HasValidationError()
        {
            // Arrange
            var query = new SearchQuery
            {
                CategoryIds = string.Empty
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryIds);
        }

        [Test]
        public void CategoryIds_Whitespace_HasValidationError()
        {
            // Arrange
            var query = new SearchQuery
            {
                CategoryIds = "   "
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryIds);
        }

        [Test]
        public void CategoryIds_WithValue_HasNoValidationError()
        {
            // Arrange
            var query = new SearchQuery
            {
                CategoryIds = "1,2,3"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CategoryIds);
        }
    }
}