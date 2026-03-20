using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using Common.Data.Profiles.Tests.FakeResolvers;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    [TestFixture]
    public class ShopProfileTests
    {
        private IMapper _mapper = null!;
        private FakeShopToEditShopModelResolver _fakeResolver = null!;
        private FakeEditShopModelToShopResolver _fakeReverseResolver = null!;

        [SetUp]
        public void SetUp()
        {
            _fakeResolver = new FakeShopToEditShopModelResolver
            {
                ReturnedValue =
                [
                    new() { Value = 99 }
                ]
            };

            _fakeReverseResolver = new FakeEditShopModelToShopResolver
            {
                ReturnedValue =
                [
                    new() { Value = 42 }
                ]
            };

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ShopProfile>();

                cfg.ConstructServicesUsing(type =>
                    type == typeof(ShopToEditShopModelResolver)
                        ? _fakeResolver
                    : type == typeof(EditShopModelToShopResolver)
                        ? _fakeReverseResolver
                        : Activator.CreateInstance(type)!);
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Shop_To_ShopModel_Maps_And_Ignores_Index_And_IsSelected()
        {
            // Arrange
            var shop = new Shop
            {
                Id = 1,
                Name = "Test Shop",
            };

            // Act
            var result = _mapper.Map<ShopModel>(shop);

            // Assert
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
            // Arrange
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
                Id = 2,
                Name = "Updated Shop",
                Index = 5,
                IsSelected = true
            };

            // Act
            var mappedShop = _mapper.Map(sourceModel, destShop);

            // Assert
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
        public void Shop_To_ShopEditModel_Uses_Fake_Resolver()
        {
            // Arrange
            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { Value = 1 }
                ]
            };

            // Act
            var result = _mapper.Map<ShopEditModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(_fakeResolver.WasCalled, Is.True);
                Assert.That(result.DisplaySequence, Has.Count.EqualTo(1));
                Assert.That(result.DisplaySequence![0].Value, Is.EqualTo(99));
            }
        }

        [Test]
        public void ShopEditModel_To_Shop_Uses_Fake_Reverse_Resolver()
        {
            // Arrange
            var editModel = new ShopEditModel
            {
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel { Value = 10 }
                ]
            };

            // Act
            var result = _mapper.Map<Shop>(editModel);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(_fakeReverseResolver.WasCalled, Is.True);
                Assert.That(result.DisplaySequence, Has.Count.EqualTo(1));
                Assert.That(result.DisplaySequence![0].Value, Is.EqualTo(42));
            }
        }
    }
}