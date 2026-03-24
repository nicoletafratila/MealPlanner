using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class RecipeToEditRecipeModelResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductCategory, ProductCategoryModel>();
                cfg.CreateMap<Product, ProductModel>();

                cfg.CreateMap<RecipeIngredient, RecipeIngredientEditModel>();

                cfg.CreateMap<Recipe, RecipeEditModel>()
                    .ForMember(
                        d => d.Ingredients,
                        opt => opt.MapFrom<
                            RecipeToEditRecipeModelResolver,
                            IList<RecipeIngredient>?>(src => src.RecipeIngredients)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenSourceIngredientsNull_ReturnsEmptyList()
        {
            var recipe = new Recipe
            {
                Id = 1,
                Name = "Test",
                RecipeIngredients = null
            };

            var result = _mapper.Map<RecipeEditModel>(recipe);

            Assert.That(result.Ingredients, Is.Not.Null);
            Assert.That(result.Ingredients, Is.Empty);
        }

        [Test]
        public void Map_WhenSourceIngredientsEmpty_ReturnsEmptyList()
        {
            var recipe = new Recipe
            {
                Id = 1,
                Name = "Test",
                RecipeIngredients = []
            };

            var result = _mapper.Map<RecipeEditModel>(recipe);

            Assert.That(result.Ingredients, Is.Not.Null);
            Assert.That(result.Ingredients, Is.Empty);
        }

        [Test]
        public void Map_MapsIngredients_OrdersByCategoryThenProductName_AndSetsIndexes()
        {
            var catA = new ProductCategory { Id = 1, Name = "Category A" };
            var catB = new ProductCategory { Id = 2, Name = "Category B" };

            var recipe = new Recipe
            {
                Id = 1,
                Name = "Complex Recipe",
                RecipeIngredients =
                [
                    new RecipeIngredient
                    {
                        Product = new Product { Id = 10, Name = "Zeta", ProductCategory = catB }
                    },
                    new RecipeIngredient
                    {
                        Product = new Product { Id = 11, Name = "Alpha", ProductCategory = catB }
                    },
                    new RecipeIngredient
                    {
                        Product = new Product { Id = 12, Name = "Beta", ProductCategory = catA }
                    },
                    new RecipeIngredient
                    {
                        Product = null   
                    }
                ]
            };

            var result = _mapper.Map<RecipeEditModel>(recipe);

            var names = result.Ingredients!.Select(i => i.Product?.Name ?? "").ToList();

            using (Assert.EnterMultipleScope())
            {
                // Expected order:
                // 1. Category A → Beta
                // 2. Category B → Alpha
                // 3. Category B → Zeta
                // 4. null product → ""
                Assert.That(names, Is.EqualTo([ "", "Beta", "Alpha", "Zeta"]).AsCollection);

                // Indexes must be 1..N
                Assert.That(
                    result.Ingredients!.Select(i => i.Index),
                    Is.EqualTo(Enumerable.Range(1, result.Ingredients!.Count)).AsCollection
                );
            }
        }
    }
}
