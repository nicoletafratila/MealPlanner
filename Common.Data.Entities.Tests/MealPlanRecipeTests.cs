namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class MealPlanRecipeTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            // Act
            var link = new MealPlanRecipe();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(link.MealPlan, Is.Null);
                Assert.That(link.MealPlanId, Is.Zero);
                Assert.That(link.Recipe, Is.Null);
                Assert.That(link.RecipeId, Is.Zero);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var mealPlan = new MealPlan();
            var recipe = new Recipe();

            // Act
            var link = new MealPlanRecipe
            {
                MealPlan = mealPlan,
                MealPlanId = 1,
                Recipe = recipe,
                RecipeId = 2
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(link.MealPlan, Is.SameAs(mealPlan));
                Assert.That(link.MealPlanId, Is.EqualTo(1));
                Assert.That(link.Recipe, Is.SameAs(recipe));
                Assert.That(link.RecipeId, Is.EqualTo(2));
            }
        }

        [Test]
        public void ToString_Contains_MealPlanId_And_RecipeId()
        {
            var link = new MealPlanRecipe
            {
                MealPlanId = 10,
                RecipeId = 20
            };

            var text = link.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("10"));
                Assert.That(text, Does.Contain("20"));
            }
        }
    }
}