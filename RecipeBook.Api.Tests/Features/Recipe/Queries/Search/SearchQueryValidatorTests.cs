using Common.Pagination;
using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Recipe.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.Search
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
        public void QueryParameters_Null_HasValidationError()
        {
            // Arrange
            var query = new SearchQuery
            {
                QueryParameters = null
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.QueryParameters);
        }

        [Test]
        public void QueryParameters_NotNull_HasNoValidationError()
        {
            // Arrange
            var query = new SearchQuery
            {
                QueryParameters = new QueryParameters<RecipeModel>()
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.QueryParameters);
        }
    }
}