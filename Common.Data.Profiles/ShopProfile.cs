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
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.DisplaySequence, opt => opt.Ignore());

            CreateMap<Shop, ShopEditModel>()
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
                .ForMember(
                    dest => dest.DisplaySequence,
                    opt => opt.MapFrom<ShopToEditShopModelResolver, IList<ShopDisplaySequence>>(src => src.DisplaySequence!)
                )
                .ReverseMap()
                .ForMember(
                    dest => dest.DisplaySequence,
                    opt => opt.MapFrom<EditShopModelToShopResolver, IList<ShopDisplaySequenceEditModel>>(src => src.DisplaySequence)
                );
        }
    }
}