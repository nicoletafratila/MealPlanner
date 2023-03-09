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
                .ReverseMap()
                .ForMember(data => data.IngredientCategory, opt => opt.Ignore())
                .ForMember(data => data.Unit, opt => opt.Ignore());
        }
    }
}
