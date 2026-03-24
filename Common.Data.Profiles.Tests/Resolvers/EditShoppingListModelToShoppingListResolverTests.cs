using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
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
                Id = 1,
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
                Id = 1,
                Name = "Test Shopping List",
                Products = []
            };

            var result = _mapper.Map<ShoppingList>(model);

            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void Map_MapsAllProducts()
        {
            var model = new ShoppingListEditModel
            {
                Id = 1,
                Name = "Groceries",
                Products =
                [
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel { Id = 10 },
                        Quantity = 2,
                        IsSelected = false
                    },
                    new ShoppingListProductEditModel
                    {
                        Product = new ProductModel{ Id = 20 },
                        Quantity = 5,
                        IsSelected = true
                    }
                ]
            };

            var result = _mapper.Map<ShoppingList>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Products!, Has.Count.EqualTo(2));

                Assert.That(result.Products![0].ProductId, Is.EqualTo(10));
                Assert.That(result.Products[0].Quantity, Is.EqualTo(2));
                Assert.That(result.Products[0].Collected, Is.False);

                Assert.That(result.Products[1].ProductId, Is.EqualTo(20));
                Assert.That(result.Products[1].Quantity, Is.EqualTo(5));
                Assert.That(result.Products[1].Collected, Is.False);
            }
        }
    }
}
