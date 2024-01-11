using AutoMapper;
using Common.Data.Entities;
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

            CreateMap<Shop, EditShopModel>()
               .ReverseMap();
        }
    }
}
