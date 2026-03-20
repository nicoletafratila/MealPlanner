using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using Common.Data.Profiles.Tests.FakeResolvers;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    [TestFixture]
    public class RecipeProfileTests
    {
        private IMapper _mapper = null!;
        private FakeRecipeToEditRecipeModelResolver _fakeForwardResolver = null!;
        private FakeEditRecipeModelToRecipeResolver _fakeReverseResolver = null!;

        [SetUp]
        public void SetUp()
        {
            _fakeForwardResolver = new FakeRecipeToEditRecipeModelResolver
            {
                ReturnedValue =
                [
                    new RecipeIngredientEditModel
                    {
                        Quantity = 5,
                        Product = new ProductModel { Name = "InjectedForward" }
                    }
                ]
            };

            _fakeReverseResolver = new FakeEditRecipeModelToRecipeResolver
            {
                ReturnedValue =
                [
                    new RecipeIngredient
                    {
                        Quantity = 3,
                        Product = new Product { Name = "InjectedReverse" }
                    }
                ]
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RecipeCategoryProfile>();
                cfg.AddProfile<RecipeProfile>();

                cfg.ConstructServicesUsing(type =>
                    type == typeof(RecipeToEditRecipeModelResolver)
                        ? _fakeForwardResolver
                    : type == typeof(EditRecipeModelToRecipeResolver)
                        ? _fakeReverseResolver
                        : Activator.CreateInstance(type)!);
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Recipe_To_RecipeModel_Maps_Category_And_Image()
        {
            var recipe = new Recipe
            {
                Id = 1,
                Name = "Test",
                ImageContent = [1, 2, 3],
                RecipeCategory = new RecipeCategory { Id = 11, Name = "Dessert" }
            };

            var result = _mapper.Map<RecipeModel>(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(1));
                Assert.That(result.Name, Is.EqualTo("Test"));
                Assert.That(result.RecipeCategoryId, Is.EqualTo("11"));
                Assert.That(result.RecipeCategoryName, Is.EqualTo("Dessert"));
                Assert.That(result.ImageUrl, Does.StartWith("data:image/jpg;base64,"));
            }
        }

        [Test]
        public void RecipeModel_To_Recipe_Ignores_RecipeCategory()
        {
            var dest = new Recipe
            {
                RecipeCategory = new RecipeCategory { Name = "OriginalCategory" }
            };

            var model = new RecipeModel
            {
                Id = 99,
                Name = "Updated"
            };

            var result = _mapper.Map(model, dest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(99));
                Assert.That(result.RecipeCategory!.Name, Is.EqualTo("OriginalCategory"));
            }
        }

        [Test]
        public void Recipe_To_RecipeEditModel_Uses_Fake_Forward_Resolver()
        {
            var recipe = new Recipe
            {
                Name = "R1",
                RecipeIngredients =
                [
                    new RecipeIngredient { Quantity = 1 }
                ]
            };

            var result = _mapper.Map<RecipeEditModel>(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fakeForwardResolver.WasCalled, Is.True);
                Assert.That(result.Ingredients, Has.Count.EqualTo(1));
                Assert.That(result.Ingredients![0].Quantity, Is.EqualTo(5));
                Assert.That(result.Ingredients[0].Product!.Name, Is.EqualTo("InjectedForward"));
            }
        }

        [Test]
        public void RecipeEditModel_To_Recipe_Uses_Fake_Reverse_Resolver()
        {
            var edit = new RecipeEditModel
            {
                Ingredients =
                [
                    new RecipeIngredientEditModel { Quantity = 10 }
                ]
            };

            var result = _mapper.Map<Recipe>(edit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fakeReverseResolver.WasCalled, Is.True);
                Assert.That(result.RecipeIngredients, Has.Count.EqualTo(1));
                Assert.That(result.RecipeIngredients![0].Quantity, Is.EqualTo(3));
                Assert.That(result.RecipeIngredients[0].Product!.Name, Is.EqualTo("InjectedReverse"));
            }
        }
    }
}