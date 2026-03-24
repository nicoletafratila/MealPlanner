using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditRecipeModelToRecipeResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UnitModel, Unit>();
                cfg.CreateMap<ProductModel, Product>();

                cfg.CreateMap<RecipeIngredientEditModel, RecipeIngredient>();

                cfg.CreateMap<RecipeEditModel, Recipe>()
                    .ForMember(
                        d => d.RecipeIngredients,
                        opt => opt.MapFrom<
                            EditRecipeModelToRecipeResolver,
                            IList<RecipeIngredientEditModel>?>(src => src.Ingredients)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenIngredientsNull_ReturnsEmptyList()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Test",
                Ingredients = null
            };

            var result = _mapper.Map<Recipe>(model);

            Assert.That(result.RecipeIngredients, Is.Not.Null);
            Assert.That(result.RecipeIngredients, Is.Empty);
        }

        [Test]
        public void Map_WhenIngredientsEmpty_ReturnsEmptyList()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Test",
                Ingredients = []
            };

            var result = _mapper.Map<Recipe>(model);

            Assert.That(result.RecipeIngredients, Is.Not.Null);
            Assert.That(result.RecipeIngredients, Is.Empty);
        }

        [Test]
        public void Map_MapsIngredientCoreFields()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Milkshake",
                Ingredients =
                [
                    new RecipeIngredientEditModel
                    {
                        RecipeId = 5,
                        Quantity = 2.5m,
                        UnitId = 3
                    }
                ]
            };

            var result = _mapper.Map<Recipe>(model);
            var ingredient = result.RecipeIngredients!.Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ingredient.RecipeId, Is.EqualTo(5));
                Assert.That(ingredient.Quantity, Is.EqualTo(2.5m));
                Assert.That(ingredient.UnitId, Is.EqualTo(3));
            }
        }

        [Test]
        public void Map_MapsOptionalUnitAndProduct()
        {
            var model = new RecipeEditModel
            {
                Id = 1,
                Name = "Smoothie",
                Ingredients =
                [
                    new RecipeIngredientEditModel
                    {
                        RecipeId = 10,
                        Quantity = 1m,
                        UnitId = 2,
                        Unit = new UnitModel { Id = 2, Name = "Cup" },
                        Product = new ProductModel { Id = 7, Name = "Strawberries" }
                    }
                ]
            };

            var result = _mapper.Map<Recipe>(model);
            var ing = result.RecipeIngredients!.Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ing.Unit, Is.Not.Null);
                Assert.That(ing.Unit!.Name, Is.EqualTo("Cup"));

                Assert.That(ing.Product, Is.Not.Null);
                Assert.That(ing.Product!.Name, Is.EqualTo("Strawberries"));
            }
        }
    }
}