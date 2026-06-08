using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles.Resolvers
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
                .Select((s, idx) =>
                {
                    var entity = context.Mapper.Map<ShopDisplaySequence>(s);
                    entity.Value = idx + 1;
                    return entity;
                })
                .ToList();
        }
    }
}
