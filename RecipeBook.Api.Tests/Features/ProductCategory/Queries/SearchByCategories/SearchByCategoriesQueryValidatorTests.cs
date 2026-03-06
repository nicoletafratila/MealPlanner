using FluentValidation.TestHelper;
using RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.SearchByCategories
{
    [TestFixture]
    public class SearchByCategoriesQueryValidatorTests
    {
        private SearchByCategoriesQueryValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new SearchByCategoriesQueryValidator();
        }

        [Test]
        public void CategoryIds_Null_HasValidationError()
        {
            // Arrange
            var query = new SearchByCategoriesQuery
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
            var query = new SearchByCategoriesQuery
            {
                CategoryIds = string.Empty
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
            var query = new SearchByCategoriesQuery
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