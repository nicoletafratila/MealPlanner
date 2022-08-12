using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanMapper : Profile
    {
        public MealPlanMapper()
        {
            CreateMap<MealPlan, MealPlanModel>()
                .ReverseMap();
        }
    }
}
