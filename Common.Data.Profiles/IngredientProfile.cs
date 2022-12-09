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
               .ForMember(model => model.Category, opt => opt.MapFrom(data => data.IngredientCategory.Name))
               .ForMember(model => model.DisplaySequence, opt => opt.MapFrom(data => data.IngredientCategory.DisplaySequence))
               .ReverseMap();

            CreateMap<Ingredient, EditIngredientModel>()
                .ReverseMap();

            CreateMap<Ingredient, RecipeIngredientModel>()
               .ForMember(model => model.Ingredient, opt => opt.MapFrom(data => data))
               .ReverseMap();
        }
    }
}
