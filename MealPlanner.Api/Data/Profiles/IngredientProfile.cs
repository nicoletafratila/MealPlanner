using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Data.Profiles
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile()
        {
            CreateMap<Ingredient, IngredientModel>()
               .ReverseMap();
        }
    }
}
