using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
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
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void RecipeIngredient_To_RecipeIngredientEditModel_Maps_Properties()
        {
            var entity = new RecipeIngredient
            {
                RecipeId = 11,
                ProductId = 22,
                UnitId = 33,
                Quantity = 4.5m
            };

            var result = _mapper.Map<RecipeIngredientEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RecipeId, Is.EqualTo(11));
                Assert.That(result.Product, Is.Null);
                Assert.That(result.UnitId, Is.EqualTo(33));
                Assert.That(result.Quantity, Is.EqualTo(4.5m));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);

                Assert.That(result.Product, Is.Null);
                Assert.That(result.Unit, Is.Null);
            }
        }

        [Test]
        public void RecipeIngredientEditModel_To_RecipeIngredient_Maps_Properties()
        {
            var model = new RecipeIngredientEditModel
            {
                RecipeId = 55,
                Product = new ProductModel() { Id = 66 },
                UnitId = 77,
                Quantity = 10
            };

            var result = _mapper.Map<RecipeIngredient>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RecipeId, Is.EqualTo(55));
                Assert.That(result.ProductId, Is.EqualTo(66));
                Assert.That(result.UnitId, Is.EqualTo(77));
                Assert.That(result.Quantity, Is.EqualTo(10));

                Assert.That(result.Recipe, Is.Null);
                Assert.That(result.Product, Is.Null);
                Assert.That(result.Unit, Is.Null);
            }
        }

        [Test]
        public void RecipeIngredient_To_ShoppingListProductEditModel_Maps_Properties()
        {
            var entity = new RecipeIngredient
            {
                ProductId = 7,
                UnitId = 8,
                Quantity = 3.25m
            };

            var result = _mapper.Map<ShoppingListProductEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Product, Is.Null);
                Assert.That(result.UnitId, Is.EqualTo(8));
                Assert.That(result.Quantity, Is.EqualTo(3.25m));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);

                Assert.That(result.Collected, Is.False);
                Assert.That(result.DisplaySequence, Is.Zero);

                Assert.That(result.Product, Is.Null);
                Assert.That(result.Unit, Is.Null);
            }
        }

        [Test]
        public void ShoppingListProductEditModel_To_RecipeIngredient_Maps_Properties()
        {
            var model = new ShoppingListProductEditModel
            {
                Product = new ProductModel() { Id = 91 },
                UnitId = 92,
                Quantity = 12.5m
            };

            var result = _mapper.Map<RecipeIngredient>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ProductId, Is.EqualTo(91));
                Assert.That(result.UnitId, Is.EqualTo(92));
                Assert.That(result.Quantity, Is.EqualTo(12.5m));

                Assert.That(result.Recipe, Is.Null);
                Assert.That(result.Product, Is.Null);
                Assert.That(result.Unit, Is.Null);
            }
        }
    }
}