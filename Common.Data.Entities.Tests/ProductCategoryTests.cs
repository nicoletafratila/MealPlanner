namespace Common.Data.Entities.Tests
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
                Assert.That(category.Id, Is.Zero);
                Assert.That(category.Name, Is.Null);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Act
            var category = new ProductCategory
            {
                Id = 7,
                Name = "Vegetables"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(category.Id, Is.EqualTo(7));
                Assert.That(category.Name, Is.EqualTo("Vegetables"));
            }
        }

        [Test]
        public void ToString_Contains_Name_And_Id()
        {
            var category = new ProductCategory
            {
                Id = 3,
                Name = "Fruits"
            };

            var text = category.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Fruits"));
                Assert.That(text, Does.Contain("3"));
            }
        }
    }
}