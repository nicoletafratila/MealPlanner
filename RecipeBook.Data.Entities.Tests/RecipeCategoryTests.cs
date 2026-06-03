namespace RecipeBook.Data.Entities.Tests
{
    [TestFixture]
    public class RecipeCategoryTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            // Act
            var category = new RecipeCategory();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(category.Id, Is.EqualTo(Guid.Empty));
                Assert.That(category.Name, Is.Null);
                Assert.That(category.DisplaySequence, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var category = new RecipeCategory
            {
                Id = id,
                Name = "Breakfast",
                DisplaySequence = 10
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(category.Id, Is.EqualTo(id));
                Assert.That(category.Name, Is.EqualTo("Breakfast"));
                Assert.That(category.DisplaySequence, Is.EqualTo(10));
            }
        }

        [Test]
        public void ToString_Contains_Name_Id_And_DisplaySequence()
        {
            var id = Guid.NewGuid();
            var category = new RecipeCategory
            {
                Id = id,
                Name = "Dinner",
                DisplaySequence = 2
            };

            var text = category.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Dinner"));
                Assert.That(text, Does.Contain(id.ToString()));
                Assert.That(text, Does.Contain("2"));
            }
        }
    }
}
