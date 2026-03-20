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
                .IgnoreBaseModelMembers()
                .ForMember(dest => dest.ProductCategory, opt => opt.MapFrom(src => src.ProductCategory));

            CreateMap<ShopDisplaySequenceEditModel, ShopDisplaySequence>()
                .ForMember(dest => dest.Shop, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCategory, opt => opt.Ignore());
        }
    }
}