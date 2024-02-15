using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class ShopToEditShopModelResolver : IMemberValueResolver<Shop, EditShopModel, IList<ShopDisplaySequence>?, IList<ShopDisplaySequenceModel>?>
    {
        public IList<ShopDisplaySequenceModel>? Resolve(Shop source, EditShopModel destination, IList<ShopDisplaySequence>? sourceValue, IList<ShopDisplaySequenceModel>? destValue, ResolutionContext context)
        {
            return source.DisplaySequence?.Select(context.Mapper.Map<ShopDisplaySequenceModel>).OrderBy(i => i.Value).ToList();
        }
    }
}
