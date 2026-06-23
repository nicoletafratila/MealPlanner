using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles.Tests
{
    [TestFixture]
    public class ShopProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ShopProfile>();
                cfg.AddProfile<ShopDisplaySequenceProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.ProductCategoryProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Shop_To_ShopModel_Maps_And_Ignores_Index_And_IsSelected()
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
            };

            var result = _mapper.Map<ShopModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(shop.Id));
                Assert.That(result.Name, Is.EqualTo(shop.Name));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ShopModel_To_Shop_Ignores_DisplaySequence_On_Destination()
        {
            var originalDisplaySequence = new List<ShopDisplaySequence>
            {
                new() { Value = 10 },
                new() { Value = 20 }
            };

            var destShop = new Shop
            {
                DisplaySequence = new List<ShopDisplaySequence>(originalDisplaySequence)
            };

            var sourceModel = new ShopModel
            {
                Id = Guid.NewGuid(),
                Name = "Updated Shop",
                Index = 5,
                IsSelected = true
            };

            var mappedShop = _mapper.Map(sourceModel, destShop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(mappedShop.Id, Is.EqualTo(sourceModel.Id));
                Assert.That(mappedShop.Name, Is.EqualTo(sourceModel.Name));
                Assert.That(mappedShop.DisplaySequence, Is.Not.Null);
                Assert.That(mappedShop.DisplaySequence!, Has.Count.EqualTo(originalDisplaySequence.Count));
                Assert.That(mappedShop.DisplaySequence!.Select(x => x.Value), Is.EqualTo(originalDisplaySequence.Select(x => x.Value)));
            }
        }

        [Test]
        public void Shop_To_ShopEditModel_Orders_By_Value_And_Sets_Index()
        {
            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { Value = 3 },
                    new() { Value = 1 },
                    new() { Value = 2 }
                ]
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.DisplaySequence, Has.Count.EqualTo(3));
                Assert.That(result.DisplaySequence![0].Value, Is.EqualTo(1));
                Assert.That(result.DisplaySequence![1].Value, Is.EqualTo(2));
                Assert.That(result.DisplaySequence![2].Value, Is.EqualTo(3));
                Assert.That(result.DisplaySequence![0].Index, Is.EqualTo(1));
                Assert.That(result.DisplaySequence![1].Index, Is.EqualTo(2));
                Assert.That(result.DisplaySequence![2].Index, Is.EqualTo(3));
            }
        }

        [Test]
        public void ShopEditModel_To_Shop_Maps_DisplaySequence()
        {
            var editModel = new ShopEditModel
            {
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel { Value = 1 },
                    new ShopDisplaySequenceEditModel { Value = 2 }
                ]
            };

            var result = _mapper.Map<Shop>(editModel);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.DisplaySequence, Is.Not.Null);
                Assert.That(result.DisplaySequence!, Has.Count.EqualTo(2));
                Assert.That(result.DisplaySequence!.Select(s => s.Value), Is.EqualTo(new[] { 1, 2 }).AsCollection);
            }
        }
    }
}
