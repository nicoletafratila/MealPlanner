using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditShopModelToShopResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShopDisplaySequenceEditModel, ShopDisplaySequence>();

                cfg.CreateMap<ShopEditModel, Shop>()
                    .ForMember(
                        d => d.DisplaySequence,
                        opt => opt.MapFrom<
                            EditShopModelToShopResolver,
                            IList<ShopDisplaySequenceEditModel>?>(src => src.DisplaySequence)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenDisplaySequenceIsNull_ReturnsEmptyList()
        {
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "Test Shop",
                DisplaySequence = null
            };

            var result = _mapper.Map<Shop>(model);

            Assert.That(result.DisplaySequence, Is.Not.Null);
            Assert.That(result.DisplaySequence, Is.Empty);
        }

        [Test]
        public void Map_WhenDisplaySequenceEmpty_ReturnsEmptyList()
        {
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "Test Shop",
                DisplaySequence = []
            };

            var result = _mapper.Map<Shop>(model);

            Assert.That(result.DisplaySequence, Is.Not.Null);
            Assert.That(result.DisplaySequence, Is.Empty);
        }

        [Test]
        public void Map_MapsItems_OrdersByValue()
        {
            var model = new ShopEditModel
            {
                Id = 1,
                Name = "Shop",
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel { Value = 20, Index = 3 },
                    new ShopDisplaySequenceEditModel { Value = 5,  Index = 1 },
                    new ShopDisplaySequenceEditModel { Value = 10, Index = 2 }
                ]
            };

            var result = _mapper.Map<Shop>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.DisplaySequence, Is.Not.Null);
                Assert.That(result.DisplaySequence!, Has.Count.EqualTo(3));

                // Ordered by Value ascending
                Assert.That(result.DisplaySequence!.Select(s => s.Value), Is.EqualTo([5, 10, 20]).AsCollection);
            }
        }
    }
}