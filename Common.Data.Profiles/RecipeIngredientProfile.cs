using AutoMapper;
using Common.Data.Entities;
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
               .ForMember(data => data.Ingredient, opt => opt.Ignore());
        }
    }
}
