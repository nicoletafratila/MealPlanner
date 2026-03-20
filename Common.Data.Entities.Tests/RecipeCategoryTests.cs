namespace Common.Data.Entities.Tests
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
                Assert.That(category.Id, Is.Zero);
                Assert.That(category.Name, Is.Null);
                Assert.That(category.DisplaySequence, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Act
            var category = new RecipeCategory
            {
                Id = 5,
                Name = "Breakfast",
                DisplaySequence = 10
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(category.Id, Is.EqualTo(5));
                Assert.That(category.Name, Is.EqualTo("Breakfast"));
                Assert.That(category.DisplaySequence, Is.EqualTo(10));
            }
        }

        [Test]
        public void ToString_Contains_Name_Id_And_DisplaySequence()
        {
            var category = new RecipeCategory
            {
                Id = 3,
                Name = "Dinner",
                DisplaySequence = 2
            };

            var text = category.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Dinner"));
                Assert.That(text, Does.Contain("3"));
                Assert.That(text, Does.Contain("2"));
            }
        }
    }
}