using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Job.Queries.GetStatus;

namespace RecipeBook.Api.Tests.Features.Job.Queries.GetStatus
{
    [TestFixture]
    public class GetStatusQueryValidatorTests
    {
        private GetStatusQueryValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new GetStatusQueryValidator();
        }

        [Test]
        public void JobId_Empty_HasValidationError()
        {
            var query = new GetStatusQuery(Guid.Empty);

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.JobId);
        }

        [Test]
        public void JobId_ValidGuid_HasNoValidationError()
        {
            var query = new GetStatusQuery(Guid.NewGuid());

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.JobId);
        }
    }
}
