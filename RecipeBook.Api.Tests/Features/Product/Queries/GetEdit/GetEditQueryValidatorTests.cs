using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Product.Queries.GetEdit;

namespace RecipeBook.Api.Tests.Features.Product.Queries.GetEdit
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
            var result = _validator.TestValidate(new GetEditQuery { Id = Guid.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Test]
        public void Id_NotEmpty_HasNoValidationError()
        {
            var result = _validator.TestValidate(new GetEditQuery { Id = Guid.NewGuid() });
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
