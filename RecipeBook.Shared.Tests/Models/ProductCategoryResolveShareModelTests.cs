using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class ProductCategoryResolveShareModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ProductCategoryResolveShareModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.CategoryIds, Is.Empty);
                Assert.That(model.TargetUserId, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public void Properties_CanBeSet()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            // Act
            var model = new ProductCategoryResolveShareModel { CategoryIds = [categoryId], TargetUserId = "user2" };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.CategoryIds, Is.EquivalentTo(new[] { categoryId }));
                Assert.That(model.TargetUserId, Is.EqualTo("user2"));
            }
        }
    }
}
