using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles
{
    public class ShopDisplaySequenceProfile : Profile
    {
        public ShopDisplaySequenceProfile()
        {
            CreateMap<ShopDisplaySequence, ShopDisplaySequenceEditModel>()
               .ReverseMap()
               .ForMember(data => data.Shop, opt => opt.Ignore())
               .ForMember(data => data.ProductCategory, opt => opt.Ignore());
        }
    }
}