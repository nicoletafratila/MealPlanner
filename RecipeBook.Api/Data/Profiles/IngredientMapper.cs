using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class IngredientMapper : Profile
    {
        public IngredientMapper()
        {
            CreateMap<Ingredient, IngredientModel>()
               .ReverseMap();
        }
    }
}
