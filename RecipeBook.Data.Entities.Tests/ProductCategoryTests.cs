namespace RecipeBook.Data.Entities.Tests
{
    [TestFixture]
    public class ProductCategoryTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            // Act
            var category = new ProductCategory();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(category.Id, Is.EqualTo(Guid.Empty));
                Assert.That(category.Name, Is.Null);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var category = new ProductCategory
            {
                Id = id,
                Name = "Vegetables"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(category.Id, Is.EqualTo(id));
                Assert.That(category.Name, Is.EqualTo("Vegetables"));
            }
        }

        [Test]
        public void ToString_Contains_Name_And_Id()
        {
            var id = Guid.NewGuid();
            var category = new ProductCategory
            {
                Id = id,
                Name = "Fruits"
            };

            var text = category.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Fruits"));
                Assert.That(text, Does.Contain(id.ToString()));
            }
        }
    }
}
