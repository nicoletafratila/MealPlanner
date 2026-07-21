using Common.Pagination;
using FluentValidation.TestHelper;
using Identity.Api.Features.ApplicationUser.Queries.Search;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.Search
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
            var query = new SearchQuery
            {
                QueryParameters = null
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.QueryParameters);
        }

        [Test]
        public void QueryParameters_NotNull_HasNoValidationError()
        {
            var query = new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>()
            };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.QueryParameters);
        }
    }
}
