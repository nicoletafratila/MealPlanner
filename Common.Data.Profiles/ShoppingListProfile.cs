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
                .ReverseMap();

            CreateMap<ShoppingList, EditShoppingListModel>()
                .ForMember(model => model.Products, opt => opt.MapFrom<ShoppingListToEditShoppingListModelResolver, IList<ShoppingListProduct>?>(data => data.Products!))
                .ReverseMap()
                .ForMember(data => data.Products, opt => opt.MapFrom<EditShoppingListModelToShoppingListResolver, IList<ShoppingListProductModel>?>(model => model.Products!));
        }
    }
}
