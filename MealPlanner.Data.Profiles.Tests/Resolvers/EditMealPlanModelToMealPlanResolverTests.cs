using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditMealPlanModelToMealPlanResolverTests
    {
        [Test]
        public void Resolve_Maps_Recipes_Correctly()
        {
            var resolver = new EditMealPlanModelToMealPlanResolver();
            ResolutionContext context = default!;

            var recipeId1 = Guid.NewGuid();
            var recipeId2 = Guid.NewGuid();
            var recipes = new List<RecipeModel>
            {
                new() { Id = recipeId1 },
                new() { Id = recipeId2 }
            };

            var id = Guid.NewGuid();
            var edit = new MealPlanEditModel
            {
                Id = id,
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
                Assert.That(result![0].RecipeId, Is.EqualTo(recipeId1));
                Assert.That(result[0].MealPlanId, Is.EqualTo(id));
                Assert.That(result[1].RecipeId, Is.EqualTo(recipeId2));
                Assert.That(result[1].MealPlanId, Is.EqualTo(id));
            }
        }

        [Test]
        public void Resolve_EmptyList_Returns_Empty()
        {
            var resolver = new EditMealPlanModelToMealPlanResolver();
            ResolutionContext context = default!;

            var edit = new MealPlanEditModel
            {
                Id = Guid.NewGuid(),
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
            ResolutionContext context = default!;

            var edit = new MealPlanEditModel
            {
                Id = Guid.NewGuid(),
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
