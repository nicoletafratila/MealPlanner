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
                .IgnoreBaseModelMembers()
                .ReverseMap()
                .ForMember(dest => dest.DisplaySequence, opt => opt.Ignore());

            CreateMap<Shop, ShopEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    dest => dest.DisplaySequence,
                    opt => opt.MapFrom<ShopToEditShopModelResolver, IList<ShopDisplaySequence>?>(src => src.DisplaySequence!)
                )
                .ReverseMap()
                .ForMember(
                    dest => dest.DisplaySequence,
                    opt => opt.MapFrom<EditShopModelToShopResolver, IList<ShopDisplaySequenceEditModel>>(src => src.DisplaySequence!)
                );
        }
    }
}