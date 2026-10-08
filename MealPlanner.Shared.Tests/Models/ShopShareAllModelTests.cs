using Common.Pagination;
using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShopShareAllModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ShopShareAllModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.TargetUserId, Is.EqualTo(string.Empty));
                Assert.That(model.Filters, Is.Null);
            }
        }

        [Test]
        public void Properties_CanBeSet()
        {
            // Arrange
            var filters = new List<FilterItem> { new("Name", "Kaufland", FilterOperator.Contains) };

            // Act
            var model = new ShopShareAllModel { TargetUserId = "user2", Filters = filters };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.TargetUserId, Is.EqualTo("user2"));
                Assert.That(model.Filters, Is.SameAs(filters));
            }
        }
    }
}
