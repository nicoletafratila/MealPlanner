using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class ShopToEditShopModelResolver : IMemberValueResolver<Shop, ShopEditModel, IList<ShopDisplaySequence>?, IList<ShopDisplaySequenceEditModel>?>
    {
        public IList<ShopDisplaySequenceEditModel>? Resolve(Shop source, ShopEditModel destination, IList<ShopDisplaySequence>? sourceValue, IList<ShopDisplaySequenceEditModel>? destValue, ResolutionContext context)
        {
            return source.DisplaySequence?.Select(context.Mapper.Map<ShopDisplaySequenceEditModel>).OrderBy(i => i.Value).ToList();
        }
    }
}
