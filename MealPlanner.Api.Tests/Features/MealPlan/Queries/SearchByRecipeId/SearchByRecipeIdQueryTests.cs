using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.SearchByRecipeId
{
    [TestFixture]
    public class SearchByRecipeIdQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesRecipeIdToZero()
        {
            var query = new SearchByRecipeIdQuery();
            Assert.That(query.RecipeId, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_SetsRecipeId()
        {
            const int id = 10;
            var query = new SearchByRecipeIdQuery(id);
            Assert.That(query.RecipeId, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_RecipeId()
        {
            var query = new SearchByRecipeIdQuery
            {
                RecipeId = 7
            };
            Assert.That(query.RecipeId, Is.EqualTo(7));
        }
    }
}