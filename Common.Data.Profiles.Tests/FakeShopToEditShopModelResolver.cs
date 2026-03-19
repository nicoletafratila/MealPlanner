using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    public class FakeShopToEditShopModelResolver
       : IMemberValueResolver<
           Shop,
           ShopEditModel,
           IList<ShopDisplaySequence>?,
           IList<ShopDisplaySequenceEditModel>>
    {
        public bool WasCalled { get; private set; }

        public IList<ShopDisplaySequenceEditModel> ReturnedValue { get; set; } = [];

        public IList<ShopDisplaySequenceEditModel> Resolve(
            Shop source,
            ShopEditModel destination,
            IList<ShopDisplaySequence>? sourceValue,
            IList<ShopDisplaySequenceEditModel>? destValue,
            ResolutionContext context)
        {
            WasCalled = true;
            return ReturnedValue;
        }
    }
}
