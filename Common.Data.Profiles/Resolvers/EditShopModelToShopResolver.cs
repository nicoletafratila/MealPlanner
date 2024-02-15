using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShopModelToShopResolver : IMemberValueResolver<EditShopModel, Shop, IList<ShopDisplaySequenceModel>?, IList<ShopDisplaySequence>?>
    {
        public IList<ShopDisplaySequence>? Resolve(EditShopModel source, Shop destination, IList<ShopDisplaySequenceModel>? sourceValue, IList<ShopDisplaySequence>? destValue, ResolutionContext context)
        {
            return source.DisplaySequence?.Select(context.Mapper.Map<ShopDisplaySequence>).ToList();
        }
    }
}
