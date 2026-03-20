using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles
{
    public class ShoppingListProfile : Profile
    {
        public ShoppingListProfile()
        {
            CreateMap<ShoppingList, ShoppingListModel>()
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Shop, opt => opt.Ignore());

            CreateMap<ShoppingList, ShoppingListEditModel>()
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore())
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