using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using Common.Data.Profiles;

namespace MealPlanner.Data.Profiles
{
    public class ShoppingListProfile : Profile
    {
        public ShoppingListProfile()
        {
            CreateMap<ShoppingList, ShoppingListModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap()
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Shop, opt => opt.Ignore());

            CreateMap<ShoppingList, ShoppingListEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    dest => dest.Products,
                    opt => opt.MapFrom<ShoppingListToEditShoppingListModelResolver, IList<ShoppingListProduct>?>(src => src.Products!)
                )
                .ReverseMap()
                .ForMember(
                    dest => dest.Products,
                    opt => opt.MapFrom<EditShoppingListModelToShoppingListResolver, IList<ShoppingListProductEditModel>>(src => src.Products!)
                );
        }
    }
}
