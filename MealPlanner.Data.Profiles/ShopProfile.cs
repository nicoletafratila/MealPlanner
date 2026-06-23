using AutoMapper;
using Common.Data.Profiles;
using Common.Models;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles
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
                    opt => opt.MapFrom(src => src.DisplaySequence == null
                        ? new List<ShopDisplaySequence>()
                        : src.DisplaySequence.OrderBy(s => s.Value).ToList())
                )
                .AfterMap((src, dest) => dest.DisplaySequence?.SetIndexes())
                .ReverseMap()
                .ForMember(dest => dest.DisplaySequence, opt => opt.MapFrom(src => src.DisplaySequence));
        }
    }
}
