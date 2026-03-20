using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests.FakeResolvers
{
    public class FakeEditShopModelToShopResolver
        : IMemberValueResolver<
            ShopEditModel,
            Shop,
            IList<ShopDisplaySequenceEditModel>?,
            IList<ShopDisplaySequence>>
    {
        public bool WasCalled { get; private set; }

        public IList<ShopDisplaySequence> ReturnedValue { get; set; } = [];

        public IList<ShopDisplaySequence> Resolve(
            ShopEditModel source,
            Shop destination,
            IList<ShopDisplaySequenceEditModel>? sourceValue,
            IList<ShopDisplaySequence>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}
