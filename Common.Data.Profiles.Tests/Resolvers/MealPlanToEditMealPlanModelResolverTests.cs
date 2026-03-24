using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class MealPlanToEditMealPlanModelResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RecipeCategory, RecipeCategoryModel>();
                cfg.CreateMap<Recipe, RecipeModel>();

                cfg.CreateMap<MealPlan, MealPlanEditModel>()
                    .ForMember(
                        d => d.Recipes,
                        opt => opt.MapFrom<
                            MealPlanToEditMealPlanModelResolver,
                            IList<MealPlanRecipe>?>(src => src.MealPlanRecipes)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenSourceRecipesNull_ReturnsEmptyList()
        {
            var mealPlan = new MealPlan
            {
                Id = 1,
                Name = "Test plan",
                MealPlanRecipes = null
            };

            var result = _mapper.Map<MealPlanEditModel>(mealPlan);

            Assert.That(result.Recipes, Is.Not.Null);
            Assert.That(result.Recipes, Is.Empty);
        }

        [Test]
        public void Map_WhenSourceRecipesEmpty_ReturnsEmptyList()
        {
            var mealPlan = new MealPlan
            {
                Id = 1,
                Name = "Test plan",
                MealPlanRecipes = []
            };

            var result = _mapper.Map<MealPlanEditModel>(mealPlan);

            Assert.That(result.Recipes, Is.Not.Null);
            Assert.That(result.Recipes, Is.Empty);
        }
        [Test]
        public void Map_MapsRecipes_OrdersByCategoryDisplaySequenceThenName_AndSetsIndexes()
        {
            var category1 = new RecipeCategory
            {
                Id = 1,
                Name = "Category 1",
                DisplaySequence = 2
            };
            var category0 = new RecipeCategory
            {
                Id = 2,
                Name = "Category 0",
                DisplaySequence = 1
            };

            var mealPlan = new MealPlan
            {
                Id = 1,
                Name = "Test plan",
                MealPlanRecipes =
                [
                    new() { Recipe = new Recipe { Name = "Zeta",  RecipeCategory = category1 } },
                    new() { Recipe = new Recipe { Name = "Alpha", RecipeCategory = category1 } },
                    new() { Recipe = new Recipe { Name = "Beta",  RecipeCategory = category0 } },
                    new() { Recipe = new Recipe { Name = "Gamma", RecipeCategory = null! } }
                ]
            };

            var result = _mapper.Map<MealPlanEditModel>(mealPlan);

            var names = result.Recipes!.Select(r => r.Name).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(names, Is.EqualTo(["Beta", "Alpha", "Zeta", "Gamma"]).AsCollection);
                Assert.That(result.Recipes!.Select(r => r.Index), Is.EqualTo(Enumerable.Range(1, result.Recipes!.Count)).AsCollection);
            }
        }
    }
}
