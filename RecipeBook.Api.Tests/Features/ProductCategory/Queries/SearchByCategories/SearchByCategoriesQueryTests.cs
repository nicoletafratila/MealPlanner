using RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.SearchByCategories
{
    [TestFixture]
    public class SearchByCategoriesQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesCategoryIdsToNull()
        {
            // Act
            var query = new SearchByCategoriesQuery();

            // Assert
            Assert.That(query.CategoryIds, Is.Null);
        }

        [Test]
        public void Ctor_SetsCategoryIds()
        {
            // Arrange
            const string ids = "1,2,3";

            // Act
            var query = new SearchByCategoriesQuery(ids);

            // Assert
            Assert.That(query.CategoryIds, Is.EqualTo(ids));
        }

        [Test]
        public void Can_Set_And_Get_CategoryIds_Property()
        {
            // Arrange
            var query = new SearchByCategoriesQuery
            {
                // Act
                CategoryIds = "4,5,6"
            };

            // Assert
            Assert.That(query.CategoryIds, Is.EqualTo("4,5,6"));
        }
    }
}