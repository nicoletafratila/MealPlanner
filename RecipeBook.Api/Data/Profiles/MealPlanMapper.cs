using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanMapper : Profile
    {
        public MealPlanMapper()
        {
            CreateMap<MealPlan, EditMealPlanModel>()
               .ForMember(model => model.Recipes, opt => opt.MapFrom(data => data.MealPlanRecipes.Select(item => item.Recipe.ToEditRecipeModel())))
               .ReverseMap()
               .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, MealPlanModel>()
                .ReverseMap()
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
