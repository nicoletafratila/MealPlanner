using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShopModelToShopResolver : IMemberValueResolver<ShopEditModel, Shop, IList<ShopDisplaySequenceEditModel>?, IList<ShopDisplaySequence>?>
    {
        public IList<ShopDisplaySequence>? Resolve(ShopEditModel source, Shop destination, IList<ShopDisplaySequenceEditModel>? sourceValue, IList<ShopDisplaySequence>? destValue, ResolutionContext context)
        {
            return source.DisplaySequence?.Select(context.Mapper.Map<ShopDisplaySequence>).ToList();
        }
    }
}
