using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles
{
    public class ShopProfile : Profile
    {
        public ShopProfile()
        {
            CreateMap<Shop, ShopModel>()
               .ReverseMap()
               .ForMember(data => data.DisplaySequence, opt => opt.Ignore());

            CreateMap<Shop, ShopEditModel>()
               .ForMember(model => model.DisplaySequence, opt => opt.MapFrom<ShopToEditShopModelResolver, IList<ShopDisplaySequence>?>(data => data.DisplaySequence))
               .ReverseMap()
               .ForMember(data => data.DisplaySequence, opt => opt.MapFrom<EditShopModelToShopResolver, IList<ShopDisplaySequenceEditModel>?>(model => model.DisplaySequence));
        }
    }
}
