using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShopModelToShopResolver()
        : IMemberValueResolver<
            ShopEditModel,
            Shop,
            IList<ShopDisplaySequenceEditModel>?,
            IList<ShopDisplaySequence>?>
    {
        public IList<ShopDisplaySequence>? Resolve(
            ShopEditModel source,
            Shop destination,
            IList<ShopDisplaySequenceEditModel>? sourceValue,
            IList<ShopDisplaySequence>? destValue,
            ResolutionContext context)
        {
            if (source.DisplaySequence == null || source.DisplaySequence.Count == 0)
                return [];

            return source.DisplaySequence
                .Select(s => context.Mapper.Map<ShopDisplaySequence>(s))
                .OrderBy(i => i.Value)
                .ToList();
        }
    }
}