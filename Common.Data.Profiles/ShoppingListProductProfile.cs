using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class ShoppingListProductProfile : Profile
    {
        public ShoppingListProductProfile()
        {
            CreateMap<ShoppingListProduct, ShoppingListProductModel>()
               .ReverseMap()
               .ForMember(data => data.ShoppingList, opt => opt.Ignore())
               .ForMember(data => data.Product, opt => opt.Ignore());

            CreateMap<RecipeIngredientModel, ShoppingListProductModel>()
               .ForMember(model => model.Id, opt => opt.MapFrom(data => data.Ingredient!.Id))
               .ForMember(model => model.Name, opt => opt.MapFrom(data => data.Ingredient!.Name))
               .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => data.Ingredient!.ImageUrl))
               .ForMember(model => model.Quantity, opt => opt.MapFrom(data => data.Quantity + "  -  " + data.Ingredient!.Unit!.Name));
        }
    }
}
