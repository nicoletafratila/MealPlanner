using Common.Data.Entities;
using Common.Models;
using MealPlanner.Shared.Models;
using AutoMapper;

namespace Common.Data.Profiles.Resolvers
{
    public class ShopToEditShopModelResolver()
        : IMemberValueResolver<
            Shop,
            ShopEditModel,
            IList<ShopDisplaySequence>?,
            IList<ShopDisplaySequenceEditModel>?>
    {
        public IList<ShopDisplaySequenceEditModel>? Resolve(
            Shop source,
            ShopEditModel destination,
            IList<ShopDisplaySequence>? sourceValue,
            IList<ShopDisplaySequenceEditModel>? destValue,
            ResolutionContext context)
        {
            if (sourceValue == null || sourceValue.Count == 0)
                return [];

            var results = sourceValue
                .Select(s => context.Mapper.Map<ShopDisplaySequenceEditModel>(s))
                .OrderBy(i => i.Value)
                .ToList();

            results.SetIndexes();
            return results;
        }
    }
}