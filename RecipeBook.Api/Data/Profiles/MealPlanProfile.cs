using AutoMapper;
using MealPlanner.Shared.Models;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanProfile : Profile
    {
        public MealPlanProfile()
        {
            CreateMap<MealPlan, MealPlanModel>()
                .ReverseMap()
                .ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, EditMealPlanModel>()
               .ForMember(model => model.Recipes, opt => opt.MapFrom<MealPlanRecipeCustomResolver, IEnumerable<MealPlanRecipe>>(data => data.MealPlanRecipes))
               .ReverseMap()
               .ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, ShoppingListModel>()
               .ForMember(model => model.Ingredients, opt => opt.MapFrom<MealPlanIngredientCustomResolver, IEnumerable<MealPlanRecipe>>(data => data.MealPlanRecipes))
               .ReverseMap()
               .ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
