using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Recipe.Queries.GetById;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetById
{
    [TestFixture]
    public class GetByIdQueryValidatorTests
    {
        private GetByIdQueryValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new GetByIdQueryValidator();
        }

        [Test]
        public void Id_Empty_HasValidationError()
        {
            var query = new GetByIdQuery { Id = Guid.Empty };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_ValidGuid_HasNoValidationError()
        {
            var query = new GetByIdQuery { Id = Guid.NewGuid() };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}