using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using Moq;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class MealPlanToEditMealPlanModelResolverTests
    {
        [Test]
        public void Resolve_Maps_And_Orders_Correctly()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var r1 = new Recipe
            {
                Id = 1,
                Name = "Banana Pie",
                RecipeCategory = new RecipeCategory { DisplaySequence = 20, Name = "Dessert" }
            };
            var r2 = new Recipe
            {
                Id = 2,
                Name = "Apple Pie",
                RecipeCategory = new RecipeCategory { DisplaySequence = 20, Name = "Dessert" }
            };
            var r3 = new Recipe
            {
                Id = 3,
                Name = "Carrot Soup",
                RecipeCategory = new RecipeCategory { DisplaySequence = 10, Name = "Soup" }
            };

            var sourceValue = new List<MealPlanRecipe>
            {
                new() { Recipe = r1 },
                new() { Recipe = r2 },
                new() { Recipe = r3 }
            };

            var mapped1 = new RecipeModel
            {
                Name = "Banana Pie",
                RecipeCategory = new RecipeCategoryModel { DisplaySequence = 20, Name = "Dessert" }
            };
            var mapped2 = new RecipeModel
            {
                Name = "Apple Pie",
                RecipeCategory = new RecipeCategoryModel { DisplaySequence = 20, Name = "Dessert" }
            };
            var mapped3 = new RecipeModel
            {
                Name = "Carrot Soup",
                RecipeCategory = new RecipeCategoryModel { DisplaySequence = 10, Name = "Soup" }
            };

            mapperMock.Setup(m => m.Map<RecipeModel>(r1)).Returns(mapped1);
            mapperMock.Setup(m => m.Map<RecipeModel>(r2)).Returns(mapped2);
            mapperMock.Setup(m => m.Map<RecipeModel>(r3)).Returns(mapped3);

            var resolver = new MealPlanToEditMealPlanModelResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var mealPlan = new MealPlan { Id = 5, MealPlanRecipes = sourceValue };

            var result = resolver.Resolve(
                mealPlan,
                new MealPlanEditModel(),
                sourceValue,
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(3));

                // Ordering: category DisplaySequence ASC, then Name ASC
                Assert.That(result![0].Name, Is.EqualTo("Carrot Soup"));
                Assert.That(result[1].Name, Is.EqualTo("Apple Pie"));
                Assert.That(result[2].Name, Is.EqualTo("Banana Pie"));

                Assert.That(result[0].Index, Is.EqualTo(1));
                Assert.That(result[1].Index, Is.EqualTo(2));
                Assert.That(result[2].Index, Is.EqualTo(3));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Null()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            var resolver = new MealPlanToEditMealPlanModelResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var empty = resolver.Resolve(
                new MealPlan { Id = 2, MealPlanRecipes = null },
                new MealPlanEditModel(),
                null,
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(empty, Is.Empty);
            }

            mapperMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Empty()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            var resolver = new MealPlanToEditMealPlanModelResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var empty = resolver.Resolve(
                new MealPlan { Id = 2, MealPlanRecipes = [] },
                new MealPlanEditModel(),
                [],
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(empty, Is.Empty);
            }

            mapperMock.VerifyNoOtherCalls();
        }
    }
}