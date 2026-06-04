using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Entities.Tests
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
                Assert.That(link.MealPlanId, Is.EqualTo(Guid.Empty));
                Assert.That(link.Recipe, Is.Null);
                Assert.That(link.RecipeId, Is.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var mealPlan = new MealPlan();
            var recipe = new Recipe();
            var mealPlanId = Guid.NewGuid();

            // Act
            var recipeId = Guid.NewGuid();
            var link = new MealPlanRecipe
            {
                MealPlan = mealPlan,
                MealPlanId = mealPlanId,
                Recipe = recipe,
                RecipeId = recipeId
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(link.MealPlan, Is.SameAs(mealPlan));
                Assert.That(link.MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(link.Recipe, Is.SameAs(recipe));
                Assert.That(link.RecipeId, Is.EqualTo(recipeId));
            }
        }

        [Test]
        public void ToString_Contains_MealPlanId_And_RecipeId()
        {
            var mealPlanId = Guid.NewGuid();
            var recipeId = Guid.NewGuid();
            var link = new MealPlanRecipe
            {
                MealPlanId = mealPlanId,
                RecipeId = recipeId
            };

            var text = link.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain(mealPlanId.ToString()));
                Assert.That(text, Does.Contain(recipeId.ToString()));
            }
        }
    }
}
