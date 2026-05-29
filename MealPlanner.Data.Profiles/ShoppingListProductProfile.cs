using AutoMapper;
using Common.Data.Profiles;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles
{
    public class ShoppingListProductProfile : Profile
    {
        public ShoppingListProductProfile()
        {
            CreateMap<ShoppingListProduct, ShoppingListProductEditModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap()
                .ForMember(dest => dest.ShoppingList, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Unit, opt => opt.Ignore());
        }
    }
}
