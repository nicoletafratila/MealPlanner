using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class ShopToEditShopModelResolverTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShopDisplaySequence, ShopDisplaySequenceEditModel>();

                cfg.CreateMap<Shop, ShopEditModel>()
                    .ForMember(
                        d => d.DisplaySequence,
                        opt => opt.MapFrom<
                            ShopToEditShopModelResolver,
                            IList<ShopDisplaySequence>?>(src => src.DisplaySequence)
                    );
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Map_WhenDisplaySequenceIsNull_ReturnsEmptyList()
        {
            var shop = new Shop
            {
                Id = 1,
                Name = "Test Shop",
                DisplaySequence = null
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            Assert.That(result.DisplaySequence, Is.Not.Null);
            Assert.That(result.DisplaySequence, Is.Empty);
        }

        [Test]
        public void Map_WhenDisplaySequenceEmpty_ReturnsEmptyList()
        {
            var shop = new Shop
            {
                Id = 1,
                Name = "Test Shop",
                DisplaySequence = []
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            Assert.That(result.DisplaySequence, Is.Not.Null);
            Assert.That(result.DisplaySequence, Is.Empty);
        }

        [Test]
        public void Map_MapsItems_OrdersByValue_AndSetsIndexes()
        {
            var shop = new Shop
            {
                Id = 1,
                Name = "Supermarket",
                DisplaySequence =
                [
                    new ShopDisplaySequence { Value = 20 },
                    new ShopDisplaySequence {    Value = 5 },
                    new ShopDisplaySequence { Value = 10 }
                ]
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            var values = result.DisplaySequence!.Select(s => s.Value).ToList();

            using (Assert.EnterMultipleScope())
            {
                // Order should be 5, 10, 20
                Assert.That(values, Is.EqualTo([5, 10, 20]).AsCollection);

                // Indexes must be 1..N after SetIndexes()
                Assert.That(
                    result.DisplaySequence!.Select(s => s.Index),
                    Is.EqualTo(Enumerable.Range(1, result.DisplaySequence!.Count)).AsCollection
                );
            }
        }
    }
}