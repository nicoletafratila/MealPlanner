using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile()
        {
            CreateMap<Ingredient, IngredientModel>()
               .ReverseMap();

            CreateMap<Ingredient, EditIngredientModel>()
                .ReverseMap();

            CreateMap<RecipeIngredient, RecipeIngredientModel>()
               .ReverseMap()
               //.ForMember(data => data.Recipe, opt => opt.Ignore())
               .ForMember(data => data.Ingredient, opt => opt.Ignore());
        }
    }
}
