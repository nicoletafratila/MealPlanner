using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    [TestFixture]
    public class ShoppingListProfileTests
    {
        private IMapper _mapper = null!;
        private FakeShoppingListToEditShoppingListResolver _fakeResolver = null!;
        private FakeEditShoppingListModelToShoppingListResolver _fakeReverseResolver = null!;

        [SetUp]
        public void SetUp()
        {
            _fakeResolver = new FakeShoppingListToEditShoppingListResolver
            {
                ReturnedValue =
                [
                    new ShoppingListProductEditModel
                    {
                        DisplaySequence = 7,
                        Product = new ProductModel { Name = "InjectedForward" }
                    }
                ]
            };

            _fakeReverseResolver = new FakeEditShoppingListModelToShoppingListResolver
            {
                ReturnedValue =
                [
                    new ShoppingListProduct
                    {
                        DisplaySequence = 3,
                        Product = new Product { Name = "InjectedReverse" }
                    }
                ]
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ShoppingListProfile>();

                cfg.ConstructServicesUsing(type =>
                    type == typeof(ShoppingListToEditShoppingListModelResolver)
                        ? _fakeResolver
                    : type == typeof(EditShoppingListModelToShoppingListResolver)
                        ? _fakeReverseResolver
                        : Activator.CreateInstance(type)!);
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ShoppingList_To_ShoppingListModel_Maps_And_Ignores_Unwanted_Fields()
        {
            var list = new ShoppingList
            {
                Id = 1,
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
                Id = 22,
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
        public void ShoppingList_To_ShoppingListEditModel_Uses_Fake_Resolver()
        {
            var list = new ShoppingList
            {
                Name = "sp1",
                Products =
                [
                    new ShoppingListProduct { Product = new Product() { Name = "P1" }, DisplaySequence = 10 }
                ]
            };

            var result = _mapper.Map<ShoppingListEditModel>(list);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fakeResolver.WasCalled, Is.True);
                Assert.That(result.Products, Has.Count.EqualTo(1));
                Assert.That(result.Products[0].DisplaySequence, Is.EqualTo(7));
                Assert.That(result.Products[0].Product!.Name, Is.EqualTo("InjectedForward"));
            }
        }

        [Test]
        public void ShoppingListEditModel_To_ShoppingList_Uses_Fake_Reverse_Resolver()
        {
            var edit = new ShoppingListEditModel
            {
                Products =
                [
                    new ShoppingListProductEditModel { DisplaySequence = 50 }
                ]
            };

            var result = _mapper.Map<ShoppingList>(edit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fakeReverseResolver.WasCalled, Is.True);
                Assert.That(result.Products, Has.Count.EqualTo(1));
                Assert.That(result.Products![0].DisplaySequence, Is.EqualTo(3));
                Assert.That(result.Products[0].Product!.Name, Is.EqualTo("InjectedReverse"));
            }
        }
    }
}