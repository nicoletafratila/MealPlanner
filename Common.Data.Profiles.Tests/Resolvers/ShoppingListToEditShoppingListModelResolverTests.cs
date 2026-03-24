using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class ShoppingListToEditShoppingListModelResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductModel>();
                cfg.CreateMap<ShoppingListProduct, ShoppingListProductEditModel>();

                cfg.CreateMap<ShoppingList, ShoppingListEditModel>()
                    .ForMember(
                        d => d.Products,
                        opt => opt.MapFrom<
                            ShoppingListToEditShoppingListModelResolver,
                            IList<ShoppingListProduct>?>(src => src.Products)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenProductsNull_ReturnsEmptyList()
        {
            var list = new ShoppingList
            {
                Id = 1,
                Name = "Test list",
                Products = null
            };

            var result = _mapper.Map<ShoppingListEditModel>(list);

            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void Map_WhenProductsEmpty_ReturnsEmptyList()
        {
            var list = new ShoppingList
            {
                Id = 1,
                Name = "Test list",
                Products = []
            };

            var result = _mapper.Map<ShoppingListEditModel>(list);

            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products, Is.Empty);
        }

        [Test]
        public void Map_MapsProducts_OrdersByCollected_ThenDisplaySequence_ThenProductName()
        {
            var list = new ShoppingList
            {
                Id = 1,
                Name = "Daily items",
                Products =
                [
                    new ShoppingListProduct
                    {
                        Collected = true,
                        DisplaySequence = 5,
                        Product = new Product { Name = "Bananas" }
                    },
                    new ShoppingListProduct
                    {
                        Collected = false,
                        DisplaySequence = 10,
                        Product = new Product { Name = "Apples" }
                    },
                    new ShoppingListProduct
                    {
                        Collected = false,
                        DisplaySequence = 3,
                        Product = new Product { Name = "Zucchini" }
                    },
                    new ShoppingListProduct
                    {
                        Collected = true,
                        DisplaySequence = 1,
                        Product = new Product { Name = "Avocado" }
                    }
                ]
            };

            var result = _mapper.Map<ShoppingListEditModel>(list);

            var orderedNames = result.Products!.Select(p => p.Product?.Name).ToList();

            using (Assert.EnterMultipleScope())
            {
                // Expected ordering:
                // 1) Collected = false → (Zucchini (3), Apples (10))
                // 2) Collected = true  → (Avocado (1), Bananas (5))
                Assert.That(
                    orderedNames,
                    Is.EqualTo(["Zucchini", "Apples", "Avocado", "Bananas"]).AsCollection
                );

                // DisplaySequence sorted within same Collected group
                Assert.That(
                    result.Products!.Select(p => p.DisplaySequence),
                    Is.EqualTo([3, 10, 1, 5]).AsCollection
                );
            }
        }
    }
}