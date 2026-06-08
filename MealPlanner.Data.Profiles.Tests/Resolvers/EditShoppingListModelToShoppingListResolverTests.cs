using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditShoppingListModelToShoppingListResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShoppingListProductEditModel, ShoppingListProduct>();
                cfg.CreateMap<ProductModel, Product>();

                cfg.CreateMap<ShoppingListEditModel, ShoppingList>()
                    .ForMember(
                        d => d.Products,
                        opt => opt.MapFrom<
                            EditShoppingListModelToShoppingListResolver,
                            IList<ShoppingListProductEditModel>?>(src => src.Products)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenProductsNull_ReturnsEmptyList()
        {
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Test Shopping List",
                Products = null
            };

            var result = _mapper.Map<ShoppingList>(model);

            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void Map_WhenProductsEmpty_ReturnsEmptyList()
        {
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Test Shopping List",
                Products = []
            };

            var result = _mapper.Map<ShoppingList>(model);

            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void Map_WhenDestinationHasExistingProducts_UpdatesInPlace()
        {
            var productId = Guid.NewGuid();
            var existingProduct = new ShoppingListProduct
            {
                ProductId = productId,
                Quantity = 1,
                Collected = false,
                DisplaySequence = 1
            };

            var destination = new ShoppingList { Products = [existingProduct] };

            var model = new ShoppingListEditModel
            {
                Products =
                [
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel { Id = productId },
                        Quantity = 3,
                        Collected = true,
                        DisplaySequence = 2
                    }
                ]
            };

            _mapper.Map(model, destination);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(destination.Products!, Has.Count.EqualTo(1));
                Assert.That(destination.Products![0], Is.SameAs(existingProduct));
                Assert.That(destination.Products[0].Collected, Is.True);
                Assert.That(destination.Products[0].Quantity, Is.EqualTo(3));
                Assert.That(destination.Products[0].DisplaySequence, Is.EqualTo(2));
            }
        }

        [Test]
        public void Map_MapsAllProducts()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var model = new ShoppingListEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Groceries",
                Products =
                [
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel { Id = id1 },
                        Quantity = 2,
                        IsSelected = false
                    },
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel{ Id = id2 },
                        Quantity = 5,
                        IsSelected = true
                    }
                ]
            };

            var result = _mapper.Map<ShoppingList>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Products!, Has.Count.EqualTo(2));

                Assert.That(result.Products![0].ProductId, Is.EqualTo(id1));
                Assert.That(result.Products[0].Quantity, Is.EqualTo(2));
                Assert.That(result.Products[0].Collected, Is.False);

                Assert.That(result.Products[1].ProductId, Is.EqualTo(id2));
                Assert.That(result.Products[1].Quantity, Is.EqualTo(5));
                Assert.That(result.Products[1].Collected, Is.False);
            }
        }
    }
}
