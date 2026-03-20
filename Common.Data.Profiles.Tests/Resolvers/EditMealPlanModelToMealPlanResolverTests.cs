using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditMealPlanModelToMealPlanResolverTests
    {
        [Test]
        public void Resolve_Maps_Recipes_Correctly()
        {
            var resolver = new EditMealPlanModelToMealPlanResolver();
            var context = default(ResolutionContext);

            var recipes = new List<RecipeModel>
            {
                new() { Id = 10 },
                new() { Id = 20 }
            };

            var edit = new MealPlanEditModel
            {
                Id = 5,
                Recipes = recipes
            };

            var result = resolver.Resolve(
                edit,
                new MealPlan(),
                recipes,
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result![0].RecipeId, Is.EqualTo(10));
                Assert.That(result[0].MealPlanId, Is.EqualTo(5));
                Assert.That(result[1].RecipeId, Is.EqualTo(20));
                Assert.That(result[1].MealPlanId, Is.EqualTo(5));
            }
        }

        [Test]
        public void Resolve_EmptyList_Returns_Empty()
        {
            var resolver = new EditMealPlanModelToMealPlanResolver();
            var context = default(ResolutionContext);

            var edit = new MealPlanEditModel
            {
                Id = 3,
                Recipes = []
            };

            var result = resolver.Resolve(
                edit,
                new MealPlan(),
                [],
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        public void Resolve_NullSourceValue_Returns_Empty()
        {
            var resolver = new EditMealPlanModelToMealPlanResolver();
            var context = default(ResolutionContext);

            var edit = new MealPlanEditModel
            {
                Id = 3,
                Recipes = null
            };

            var result = resolver.Resolve(
                edit,
                new MealPlan(),
                null,
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Empty);
            }
        }
    }
}