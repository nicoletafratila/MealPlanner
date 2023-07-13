using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class RecipeIngredientProfile : Profile
    {
        public RecipeIngredientProfile()
        {
            CreateMap<RecipeIngredient, RecipeIngredientModel>()
               .ReverseMap()
               .ForMember(data => data.Recipe, opt => opt.Ignore())
               .ForMember(data => data.Product, opt => opt.Ignore());

            CreateMap<RecipeIngredientModel, ShoppingListProductModel>()
               .ForMember(data => data.Collected, opt => opt.Ignore())
               .ReverseMap();
        }
    }
}
