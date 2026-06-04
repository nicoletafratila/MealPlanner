using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests
{
    public class ShoppingListProductProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<ShoppingListProductProfile>();
                c.AddProfile<RecipeBook.Data.Profiles.UnitProfile>();
                c.AddProfile<RecipeBook.Data.Profiles.ProductProfile>();
                c.AddProfile<RecipeBook.Data.Profiles.ProductCategoryProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ShoppingListProduct_To_ShoppingListProductEditModel_Maps_All_Properties()
        {
            var unitId = Guid.NewGuid();
            var entity = new ShoppingListProduct
            {
                ShoppingListId = Guid.NewGuid(),
                Quantity = 2.5m,
                UnitId = unitId,
                ProductId = 99,
                Collected = true,
                DisplaySequence = 4
            };

            var result = _mapper.Map<ShoppingListProductEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ShoppingListId, Is.EqualTo(entity.ShoppingListId));
                Assert.That(result.Quantity, Is.EqualTo(entity.Quantity));
                Assert.That(result.UnitId, Is.EqualTo(entity.UnitId));
                Assert.That(result.Product, Is.Null);
                Assert.That(result.Collected, Is.EqualTo(entity.Collected));
                Assert.That(result.DisplaySequence, Is.EqualTo(entity.DisplaySequence));

                Assert.That(result.Unit, Is.Null);
                Assert.That(result.Product, Is.Null);
            }
        }

        [Test]
        public void ShoppingListProductEditModel_To_ShoppingListProduct_Maps_All_Properties()
        {
            var unitId = Guid.NewGuid();
            var model = new ShoppingListProductEditModel
            {
                ShoppingListId = Guid.NewGuid(),
                Quantity = 11.75m,
                UnitId = unitId,
                Product = new ProductModel() { Id = 41 },
                Collected = false,
                DisplaySequence = 2
            };

            var result = _mapper.Map<ShoppingListProduct>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ShoppingListId, Is.EqualTo(model.ShoppingListId));
                Assert.That(result.Quantity, Is.EqualTo(model.Quantity));
                Assert.That(result.UnitId, Is.EqualTo(model.UnitId));
                Assert.That(result.ProductId, Is.EqualTo(model.Product.Id));
                Assert.That(result.Collected, Is.EqualTo(model.Collected));
                Assert.That(result.DisplaySequence, Is.EqualTo(model.DisplaySequence));

                Assert.That(result.ShoppingList, Is.Null);
                Assert.That(result.Unit, Is.Null);
                Assert.That(result.Product, Is.Null);
            }
        }

        [Test]
        public void Null_SourceValue_Does_Not_Overwrite_Destination()
        {
            var shoppingListId = Guid.NewGuid();
            var destUnitId = Guid.NewGuid();
            var model = new ShoppingListProductEditModel
            {
                Quantity = 0,
                UnitId = Guid.NewGuid(),
                Product = new ProductModel() { Id = 1 },
                DisplaySequence = 1,
                ShoppingListId = shoppingListId,
            };

            var destination = new ShoppingListProduct
            {
                ShoppingListId = shoppingListId,
                Quantity = 10,
                UnitId = destUnitId,
                ProductId = 8,
                Collected = false,
                DisplaySequence = 10,
                Product = new Product { Name = "Existing Product" },
                Unit = new Unit { Name = "Existing Unit" }
            };

            _mapper.Map(model, destination);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(destination.Quantity, Is.EqualTo(model.Quantity));
                Assert.That(destination.UnitId, Is.EqualTo(model.UnitId));
                Assert.That(destination.ProductId, Is.EqualTo(model.Product!.Id));
                Assert.That(destination.DisplaySequence, Is.EqualTo(model.DisplaySequence));
                Assert.That(destination.ShoppingListId, Is.EqualTo(model.ShoppingListId));

                Assert.That(destination.Product!.Name, Is.EqualTo("Existing Product"));
                Assert.That(destination.Unit!.Name, Is.EqualTo("Existing Unit"));
            }
        }
    }
}
