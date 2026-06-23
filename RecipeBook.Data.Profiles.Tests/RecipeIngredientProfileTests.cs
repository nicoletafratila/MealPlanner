using AutoMapper;
using MealPlanner.Data.Profiles;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
{
    [TestFixture]
    public class RecipeIngredientProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RecipeIngredientProfile>();
                cfg.AddProfile<UnitProfile>();
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<ShoppingListProductProfile>();
                cfg.AddProfile<ProductCategoryProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void RecipeIngredient_To_RecipeIngredientEditModel_Maps_Properties()
        {
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var recipeId = Guid.NewGuid();
            var entity = new RecipeIngredient
            {
                RecipeId = recipeId,
                ProductId = productId,
                UnitId = unitId,
                Quantity = 4.5m
            };

            var result = _mapper.Map<RecipeIngredientEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RecipeId, Is.EqualTo(recipeId));
                Assert.That(result.Product, Is.Null);
                Assert.That(result.UnitId, Is.EqualTo(unitId));
                Assert.That(result.Quantity, Is.EqualTo(4.5m));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
                Assert.That(result.Unit, Is.Null);
            }
        }

        [Test]
        public void RecipeIngredientEditModel_To_RecipeIngredient_Maps_Properties()
        {
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var recipeId = Guid.NewGuid();
            var model = new RecipeIngredientEditModel
            {
                RecipeId = recipeId,
                Product = new ProductModel() { Id = productId },
                UnitId = unitId,
                Quantity = 10
            };

            var result = _mapper.Map<RecipeIngredient>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RecipeId, Is.EqualTo(recipeId));
                Assert.That(result.ProductId, Is.EqualTo(productId));
                Assert.That(result.UnitId, Is.EqualTo(unitId));
                Assert.That(result.Quantity, Is.EqualTo(10));
                Assert.That(result.Recipe, Is.Null);
                Assert.That(result.Product, Is.Null);
                Assert.That(result.Unit, Is.Null);
            }
        }

        [Test]
        public void RecipeIngredient_To_ShoppingListProductEditModel_Maps_Properties()
        {
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var entity = new RecipeIngredient
            {
                ProductId = productId,
                UnitId = unitId,
                Quantity = 3.25m
            };

            var result = _mapper.Map<ShoppingListProductEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Product, Is.Null);
                Assert.That(result.UnitId, Is.EqualTo(unitId));
                Assert.That(result.Quantity, Is.EqualTo(3.25m));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
                Assert.That(result.Collected, Is.False);
                Assert.That(result.DisplaySequence, Is.Zero);
                Assert.That(result.Unit, Is.Null);
            }
        }

        [Test]
        public void ShoppingListProductEditModel_To_RecipeIngredient_Maps_Properties()
        {
            var productId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var model = new ShoppingListProductEditModel
            {
                Product = new ProductModel() { Id = productId },
                UnitId = unitId,
                Quantity = 12.5m
            };

            var result = _mapper.Map<RecipeIngredient>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ProductId, Is.EqualTo(productId));
                Assert.That(result.UnitId, Is.EqualTo(unitId));
                Assert.That(result.Quantity, Is.EqualTo(12.5m));
                Assert.That(result.Recipe, Is.Null);
                Assert.That(result.Product, Is.Null);
                Assert.That(result.Unit, Is.Null);
            }
        }
    }
}
