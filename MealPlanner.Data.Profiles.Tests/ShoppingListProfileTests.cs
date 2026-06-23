using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests
{
    [TestFixture]
    public class ShoppingListProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ShoppingListProfile>();
                cfg.AddProfile<ShoppingListProductProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.UnitProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.ProductProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.ProductCategoryProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ShoppingList_To_ShoppingListModel_Maps_And_Ignores_Unwanted_Fields()
        {
            var list = new ShoppingList
            {
                Id = Guid.NewGuid(),
                Name = "Weekly",
                Shop = new Shop(),
                Products =
                [
                    new ShoppingListProduct { DisplaySequence = 1 }
                ]
            };

            var model = _mapper.Map<ShoppingListModel>(list);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Id, Is.EqualTo(list.Id));
                Assert.That(model.Name, Is.EqualTo(list.Name));
            }
        }

        [Test]
        public void ShoppingListModel_To_ShoppingList_Ignores_Products_And_Shop()
        {
            var originalProducts =
                new List<ShoppingListProduct> { new() { DisplaySequence = 1 } };

            var dest = new ShoppingList
            {
                Products = new List<ShoppingListProduct>(originalProducts),
                Shop = new Shop { Name = "Original Shop" }
            };

            var model = new ShoppingListModel
            {
                Id = Guid.NewGuid(),
                Name = "Updated"
            };

            var result = _mapper.Map(model, dest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo(model.Name));

                Assert.That(result.Products, Has.Count.EqualTo(originalProducts.Count));
                Assert.That(result.Shop!.Name, Is.EqualTo("Original Shop"));
            }
        }

        [Test]
        public void ShoppingList_To_ShoppingListEditModel_WhenProductsNull_ReturnsEmptyList()
        {
            var list = new ShoppingList { Name = "Test", Products = null };

            var result = _mapper.Map<ShoppingListEditModel>(list);

            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void ShoppingList_To_ShoppingListEditModel_OrdersByCollected_ThenDisplaySequence_ThenProductName()
        {
            var list = new ShoppingList
            {
                Name = "Daily items",
                Products =
                [
                    new ShoppingListProduct { Collected = true,  DisplaySequence = 5,  Product = new Product { Name = "Bananas" } },
                    new ShoppingListProduct { Collected = false, DisplaySequence = 10, Product = new Product { Name = "Apples" } },
                    new ShoppingListProduct { Collected = false, DisplaySequence = 3,  Product = new Product { Name = "Zucchini" } },
                    new ShoppingListProduct { Collected = true,  DisplaySequence = 1,  Product = new Product { Name = "Avocado" } }
                ]
            };

            var result = _mapper.Map<ShoppingListEditModel>(list);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(
                    result.Products!.Select(p => p.Product?.Name),
                    Is.EqualTo(["Zucchini", "Apples", "Avocado", "Bananas"]).AsCollection
                );
                Assert.That(
                    result.Products!.Select(p => p.DisplaySequence),
                    Is.EqualTo([3, 10, 1, 5]).AsCollection
                );
            }
        }

        [Test]
        public void ShoppingListEditModel_To_ShoppingList_WhenProductsNull_ReturnsEmptyList()
        {
            var edit = new ShoppingListEditModel { Products = null };

            var result = _mapper.Map<ShoppingList>(edit);

            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void ShoppingListEditModel_To_ShoppingList_WhenProductsEmpty_ReturnsEmptyList()
        {
            var edit = new ShoppingListEditModel { Products = [] };

            var result = _mapper.Map<ShoppingList>(edit);

            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void ShoppingListEditModel_To_ShoppingList_NewProduct_CreatesMappedProduct()
        {
            var productId = Guid.NewGuid();
            var edit = new ShoppingListEditModel
            {
                Products =
                [
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel { Id = productId },
                        Quantity = 2,
                        DisplaySequence = 1
                    }
                ]
            };

            var result = _mapper.Map<ShoppingList>(edit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Products, Has.Count.EqualTo(1));
                Assert.That(result.Products![0].Quantity, Is.EqualTo(2));
                Assert.That(result.Products[0].DisplaySequence, Is.EqualTo(1));
            }
        }

        [Test]
        public void ShoppingListEditModel_To_ShoppingList_ExistingProduct_UpdatesInPlace()
        {
            var productId = Guid.NewGuid();
            var existing = new ShoppingListProduct
            {
                ProductId = productId,
                Quantity = 1,
                DisplaySequence = 5
            };

            var edit = new ShoppingListEditModel
            {
                Products =
                [
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel { Id = productId },
                        Quantity = 3,
                        DisplaySequence = 10
                    }
                ]
            };

            var dest = new ShoppingList { Products = [existing] };
            _mapper.Map(edit, dest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(dest.Products, Has.Count.EqualTo(1));
                Assert.That(dest.Products![0], Is.SameAs(existing));
                Assert.That(dest.Products[0].Quantity, Is.EqualTo(3));
                Assert.That(dest.Products[0].DisplaySequence, Is.EqualTo(10));
            }
        }
    }
}
