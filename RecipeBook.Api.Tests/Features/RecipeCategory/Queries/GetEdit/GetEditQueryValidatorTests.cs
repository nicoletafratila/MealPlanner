using FluentValidation.TestHelper;
using RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Queries.GetEdit
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
            // Arrange
            var query = new GetEditQuery { Id = 0 };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_Negative_HasValidationError()
        {
            // Arrange
            var query = new GetEditQuery { Id = -1 };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_GreaterThanZero_HasNoValidationError()
        {
            // Arrange
            var query = new GetEditQuery { Id = 5 };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}