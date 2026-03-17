using FluentValidation.TestHelper;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.GetEdit
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
        public void Name_Null_HasValidationError()
        {
            var query = new GetEditQuery { Name = null };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Test]
        public void Name_Empty_HasValidationError()
        {
            var query = new GetEditQuery { Name = "" };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Test]
        public void Name_Whitespace_HasValidationError()
        {
            var query = new GetEditQuery { Name = "   " };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Test]
        public void Name_Valid_HasNoValidationError()
        {
            var query = new GetEditQuery { Name = "alice" };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}