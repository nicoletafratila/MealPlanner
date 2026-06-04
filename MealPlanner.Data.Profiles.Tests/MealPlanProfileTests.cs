using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Data.Profiles.Resolvers;
using MealPlanner.Data.Profiles.Tests.FakeResolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests
{
    [TestFixture]
    public class MealPlanProfileTests
    {
        private IMapper _mapper = null!;
        private FakeMealPlanToEditMealPlanModelResolver _fakeForward = null!;
        private FakeEditMealPlanModelToMealPlanResolver _fakeReverse = null!;
        private static readonly Guid ReverseMealPlanId = Guid.NewGuid();
        private static readonly Guid ReverseRecipeId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _fakeForward = new FakeMealPlanToEditMealPlanModelResolver
            {
                ReturnedValue =
                [
                    new RecipeModel
                    {
                        Name = "InjectedForward",
                        Index = 5
                    }
                ]
            };

            _fakeReverse = new FakeEditMealPlanModelToMealPlanResolver
            {
                ReturnedValue =
                [
                    new MealPlanRecipe
                    {
                        RecipeId = ReverseRecipeId,
                        MealPlanId = ReverseMealPlanId
                    }
                ]
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MealPlanProfile>();

                cfg.ConstructServicesUsing(type =>
                    type == typeof(MealPlanToEditMealPlanModelResolver)
                        ? _fakeForward
                    : type == typeof(EditMealPlanModelToMealPlanResolver)
                        ? _fakeReverse
                        : Activator.CreateInstance(type)!);
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void MealPlan_To_MealPlanModel_Maps_Id_And_Name()
        {
            var id = Guid.NewGuid();
            var mp = new MealPlan
            {
                Id = id,
                Name = "Weekly Plan"
            };

            var result = _mapper.Map<MealPlanModel>(mp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Weekly Plan"));
            }
        }

        [Test]
        public void MealPlanModel_To_MealPlan_Does_Not_Overwrite_Other_Fields()
        {
            var dest = new MealPlan
            {
                Name = "Original"
            };

            var id = Guid.NewGuid();
            var model = new MealPlanModel
            {
                Id = id,
                Name = "Updated"
            };

            var result = _mapper.Map(model, dest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Updated"));
            }
        }

        [Test]
        public void MealPlan_To_MealPlanEditModel_Uses_Fake_ForwardResolver()
        {
            var mp = new MealPlan
            {
                Id = Guid.NewGuid(),
                MealPlanRecipes =
                [
                    new MealPlanRecipe
                    {
                        Recipe = new Recipe { Name = "OriginalRecipe" }
                    }
                ]
            };

            var result = _mapper.Map<MealPlanEditModel>(mp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fakeForward.WasCalled, Is.True);
                Assert.That(result.Recipes, Has.Count.EqualTo(1));
                Assert.That(result.Recipes![0].Name, Is.EqualTo("InjectedForward"));
                Assert.That(result.Recipes[0].Index, Is.EqualTo(5));
            }
        }

        [Test]
        public void MealPlanEditModel_To_MealPlan_Uses_Fake_ReverseResolver()
        {
            var edit = new MealPlanEditModel
            {
                Id = ReverseMealPlanId,
                Recipes =
                [
                    new RecipeModel
                    {
                        Name = "OriginalEditRecipe"
                    }
                ]
            };

            var result = _mapper.Map<MealPlan>(edit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fakeReverse.WasCalled, Is.True);
                Assert.That(result.MealPlanRecipes, Has.Count.EqualTo(1));
                Assert.That(result.MealPlanRecipes![0].RecipeId, Is.EqualTo(ReverseRecipeId));
                Assert.That(result.MealPlanRecipes[0].MealPlanId, Is.EqualTo(ReverseMealPlanId));
            }
        }
    }
}
