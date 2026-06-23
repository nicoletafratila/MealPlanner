using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
{
    [TestFixture]
    public class RecipeProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RecipeCategoryProfile>();
                cfg.AddProfile<RecipeProfile>();
                cfg.AddProfile<RecipeIngredientProfile>();
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<ProductCategoryProfile>();
                cfg.AddProfile<UnitProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Recipe_To_RecipeModel_Maps_Category_And_Image()
        {
            var categoryId = Guid.NewGuid();
            var recipeId = Guid.NewGuid();
            var recipe = new Recipe
            {
                Id = recipeId,
                Name = "Test",
                ImageContent = [1, 2, 3],
                RecipeCategory = new RecipeCategory { Id = categoryId, Name = "Dessert" }
            };

            var result = _mapper.Map<RecipeModel>(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(recipeId));
                Assert.That(result.Name, Is.EqualTo("Test"));
                Assert.That(result.RecipeCategoryId, Is.EqualTo(categoryId.ToString()));
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

            var modelId = Guid.NewGuid();
            var model = new RecipeModel { Id = modelId, Name = "Updated" };

            var result = _mapper.Map(model, dest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(modelId));
                Assert.That(result.RecipeCategory!.Name, Is.EqualTo("OriginalCategory"));
            }
        }

        [Test]
        public void Recipe_To_RecipeEditModel_WhenIngredientsNull_ReturnsEmptyList()
        {
            var recipe = new Recipe { Name = "Test", RecipeIngredients = null };

            var result = _mapper.Map<RecipeEditModel>(recipe);

            Assert.That(result.Ingredients, Is.Empty);
        }

        [Test]
        public void Recipe_To_RecipeEditModel_OrdersByCategoryThenName_AndSetsIndexes()
        {
            var catA = new ProductCategory { Name = "Category A" };
            var catB = new ProductCategory { Name = "Category B" };

            var recipe = new Recipe
            {
                Name = "Test Recipe",
                RecipeIngredients =
                [
                    new RecipeIngredient { Product = new Product { Name = "Zeta",  ProductCategory = catB } },
                    new RecipeIngredient { Product = new Product { Name = "Alpha", ProductCategory = catB } },
                    new RecipeIngredient { Product = new Product { Name = "Beta",  ProductCategory = catA } },
                    new RecipeIngredient { Product = null }
                ]
            };

            var result = _mapper.Map<RecipeEditModel>(recipe);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(
                    result.Ingredients!.Select(i => i.Product?.Name ?? ""),
                    Is.EqualTo(["", "Beta", "Alpha", "Zeta"]).AsCollection
                );
                Assert.That(
                    result.Ingredients!.Select(i => i.Index),
                    Is.EqualTo(Enumerable.Range(1, result.Ingredients!.Count)).AsCollection
                );
            }
        }

        [Test]
        public void RecipeEditModel_To_Recipe_WhenIngredientsNull_ReturnsEmptyList()
        {
            var edit = new RecipeEditModel { Name = "Test", Ingredients = null };

            var result = _mapper.Map<Recipe>(edit);

            Assert.That(result.RecipeIngredients, Is.Empty);
        }

        [Test]
        public void RecipeEditModel_To_Recipe_Maps_Ingredients()
        {
            var recipeId = Guid.NewGuid();
            var unitId = Guid.NewGuid();

            var edit = new RecipeEditModel
            {
                Name = "Milkshake",
                Ingredients =
                [
                    new RecipeIngredientEditModel
                    {
                        RecipeId = recipeId,
                        Quantity = 2.5m,
                        UnitId = unitId
                    }
                ]
            };

            var result = _mapper.Map<Recipe>(edit);
            var ingredient = result.RecipeIngredients!.Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ingredient.RecipeId, Is.EqualTo(recipeId));
                Assert.That(ingredient.Quantity, Is.EqualTo(2.5m));
                Assert.That(ingredient.UnitId, Is.EqualTo(unitId));
            }
        }
    }
}
