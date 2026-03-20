using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
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
                c.AddProfile<UnitProfile>();
                c.AddProfile<ProductProfile>();
                c.AddProfile<ProductCategoryProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ShoppingListProduct_To_ShoppingListProductEditModel_Maps_All_Properties()
        {
            var entity = new ShoppingListProduct
            {
                ShoppingListId = 5,
                Quantity = 2.5m,
                UnitId = 10,
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

                // Navigation properties are not mapped
                Assert.That(result.Unit, Is.Null);
                Assert.That(result.Product, Is.Null);
            }
        }

        [Test]
        public void ShoppingListProductEditModel_To_ShoppingListProduct_Maps_All_Properties()
        {
            var model = new ShoppingListProductEditModel
            {
                ShoppingListId = 8,
                Quantity = 11.75m,
                UnitId = 3,
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

                // These must remain null due to .Ignore()
                Assert.That(result.ShoppingList, Is.Null);
                Assert.That(result.Unit, Is.Null);
                Assert.That(result.Product, Is.Null);
            }
        }

        [Test]
        public void Null_SourceValue_Does_Not_Overwrite_Destination()
        {
            var model = new ShoppingListProductEditModel
            {
                Quantity = 0,   // Zero is valid, not null
                UnitId = 1,
                Product = new ProductModel() { Id = 1 },
                DisplaySequence = 1,
                ShoppingListId = 1,
            };

            var destination = new ShoppingListProduct
            {
                ShoppingListId = 1,
                Quantity = 10,
                UnitId = 5,
                ProductId = 8,
                Collected = false,
                DisplaySequence = 10,
                Product = new Product { Name = "Existing Product" },
                Unit = new Unit { Name = "Existing Unit" }
            };

            _mapper.Map(model, destination);

            using (Assert.EnterMultipleScope())
            {
                // Scalar values are overwritten by mapping
                Assert.That(destination.Quantity, Is.EqualTo(model.Quantity));
                Assert.That(destination.UnitId, Is.EqualTo(model.UnitId));
                Assert.That(destination.ProductId, Is.EqualTo(model.Product!.Id));
                Assert.That(destination.DisplaySequence, Is.EqualTo(model.DisplaySequence));
                Assert.That(destination.ShoppingListId, Is.EqualTo(model.ShoppingListId));

                // Navigation properties must stay untouched
                Assert.That(destination.Product!.Name, Is.EqualTo("Existing Product"));
                Assert.That(destination.Unit!.Name, Is.EqualTo("Existing Unit"));
            }
        }
    }
}