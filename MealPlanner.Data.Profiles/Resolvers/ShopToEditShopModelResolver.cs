using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles.Resolvers
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

            return sourceValue
                .OrderBy(s => s.Value)
                .Select((s, i) => { var m = context.Mapper.Map<ShopDisplaySequenceEditModel>(s); m.Index = i + 1; return m; })
                .ToList();
        }
    }
}
