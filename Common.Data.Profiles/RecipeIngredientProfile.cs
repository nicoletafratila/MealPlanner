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
            CreateMap<RecipeIngredient, RecipeIngredientEditModel>()
               .ReverseMap()
               .ForMember(data => data.Recipe, opt => opt.Ignore())
               .ForMember(data => data.Product, opt => opt.Ignore())
               .ForMember(data => data.Unit, opt => opt.Ignore()); 

            CreateMap<RecipeIngredient, ShoppingListProductEditModel>()
               .ForMember(data => data.Collected, opt => opt.Ignore())
               .ForMember(data => data.DisplaySequence, opt => opt.Ignore())
               .ReverseMap()
               .ForMember(data => data.Recipe, opt => opt.Ignore())
               .ForMember(data => data.Product, opt => opt.Ignore())
               .ForMember(data => data.Unit, opt => opt.Ignore());
        }
    }
}
