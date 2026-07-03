using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.SearchByRecipeId
{
    [TestFixture]
    public class SearchByRecipeIdQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesRecipeIdToEmpty()
        {
            var query = new SearchByRecipeIdQuery();
            Assert.That(query.RecipeId, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Ctor_SetsRecipeId()
        {
            var id = Guid.NewGuid();
            var query = new SearchByRecipeIdQuery(id);
            Assert.That(query.RecipeId, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_RecipeId()
        {
            var id = Guid.NewGuid();
            var query = new SearchByRecipeIdQuery
            {
                RecipeId = id
            };
            Assert.That(query.RecipeId, Is.EqualTo(id));
        }
    }
}