using FluentValidation.TestHelper;
using RecipeBook.Api.Features.ProductCategory.Queries.GetEdit;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.GetEdit
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
        public void Id_Empty_HasValidationError()
        {
            // Arrange
            var query = new GetEditQuery { Id = Guid.Empty };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_NotEmpty_HasNoValidationError()
        {
            // Arrange
            var query = new GetEditQuery { Id = Guid.NewGuid() };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}