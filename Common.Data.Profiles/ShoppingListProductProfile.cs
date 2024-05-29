using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles
{
    public class ShoppingListProductProfile : Profile
    {
        public ShoppingListProductProfile()
        {
            CreateMap<ShoppingListProduct, ShoppingListProductEditModel>()
               .ReverseMap()
               .ForMember(data => data.ShoppingList, opt => opt.Ignore())
               .ForMember(data => data.Product, opt => opt.Ignore())
               .ForMember(data => data.Unit, opt => opt.Ignore());
        }
    }
}
