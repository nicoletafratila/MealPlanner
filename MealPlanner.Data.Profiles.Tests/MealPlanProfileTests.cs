using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests
{
    [TestFixture]
    public class MealPlanProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MealPlanProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.RecipeProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.RecipeIngredientProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.RecipeCategoryProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.ProductCategoryProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.ProductProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.UnitProfile>();
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
            var dest = new MealPlan { Name = "Original" };
            var id = Guid.NewGuid();
            var model = new MealPlanModel { Id = id, Name = "Updated" };

            var result = _mapper.Map(model, dest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Updated"));
            }
        }

        [Test]
        public void MealPlan_To_MealPlanEditModel_Orders_By_Category_Then_Name_And_Sets_Index()
        {
            var catA = new RecipeCategory { DisplaySequence = 1, Name = "Cat A" };
            var catB = new RecipeCategory { DisplaySequence = 2, Name = "Cat B" };

            var mp = new MealPlan
            {
                Id = Guid.NewGuid(),
                MealPlanRecipes =
                [
                    new MealPlanRecipe { Recipe = new Recipe { Name = "Zucchini", RecipeCategory = catB } },
                    new MealPlanRecipe { Recipe = new Recipe { Name = "Apple",    RecipeCategory = catA } },
                    new MealPlanRecipe { Recipe = new Recipe { Name = "Banana",   RecipeCategory = catA } },
                ]
            };

            var result = _mapper.Map<MealPlanEditModel>(mp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Recipes, Has.Count.EqualTo(3));
                Assert.That(result.Recipes![0].Name, Is.EqualTo("Apple"));
                Assert.That(result.Recipes![1].Name, Is.EqualTo("Banana"));
                Assert.That(result.Recipes![2].Name, Is.EqualTo("Zucchini"));
                Assert.That(result.Recipes![0].Index, Is.EqualTo(1));
                Assert.That(result.Recipes![1].Index, Is.EqualTo(2));
                Assert.That(result.Recipes![2].Index, Is.EqualTo(3));
            }
        }

        [Test]
        public void MealPlanEditModel_To_MealPlan_WhenRecipesNull_ReturnsEmptyMealPlanRecipes()
        {
            var edit = new MealPlanEditModel { Id = Guid.NewGuid(), Recipes = null };

            var result = _mapper.Map<MealPlan>(edit);

            Assert.That(result.MealPlanRecipes, Is.Empty);
        }

        [Test]
        public void MealPlanEditModel_To_MealPlan_Maps_RecipeIds_And_MealPlanId()
        {
            var mealPlanId = Guid.NewGuid();
            var recipeId1 = Guid.NewGuid();
            var recipeId2 = Guid.NewGuid();

            var edit = new MealPlanEditModel
            {
                Id = mealPlanId,
                Recipes =
                [
                    new RecipeModel { Id = recipeId1 },
                    new RecipeModel { Id = recipeId2 }
                ]
            };

            var result = _mapper.Map<MealPlan>(edit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.MealPlanRecipes, Has.Count.EqualTo(2));
                Assert.That(result.MealPlanRecipes![0].RecipeId, Is.EqualTo(recipeId1));
                Assert.That(result.MealPlanRecipes[0].MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(result.MealPlanRecipes[1].RecipeId, Is.EqualTo(recipeId2));
                Assert.That(result.MealPlanRecipes[1].MealPlanId, Is.EqualTo(mealPlanId));
                Assert.That(result.MealPlanRecipes[0].Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(result.MealPlanRecipes[1].Id, Is.Not.EqualTo(Guid.Empty));
            }
        }
    }
}
