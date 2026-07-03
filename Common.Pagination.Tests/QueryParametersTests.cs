using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class QueryParametersTests
    {
        [Test]
        public void Ctor_Normalizes_PageNumber_And_PageSize()
        {
            // Act
            var parameters = new QueryParameters<RecipeModel>();

            // Assert: we only assert invariants we control
            using (Assert.EnterMultipleScope())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(parameters.PageNumber, Is.GreaterThanOrEqualTo(1));
                    Assert.That(parameters.PageSize, Is.GreaterThan(0));
                }
            }
        }
    }
}
