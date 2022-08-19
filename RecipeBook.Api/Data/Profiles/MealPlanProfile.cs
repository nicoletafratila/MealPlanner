using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanProfile : Profile
    {
        public MealPlanProfile()
        {
            CreateMap<MealPlan, MealPlanModel>()
                .ReverseMap()
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, EditMealPlanModel>()
               .ForMember(model => model.Recipes, opt => opt.MapFrom<MealPlanRecipeCustomResolver, IEnumerable<MealPlanRecipe>>(data => data.MealPlanRecipes))
               .ReverseMap()
               .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, ShoppingListModel>()
               .ForMember(model => model.Ingredients, opt => opt.MapFrom<MealPlanIngredientCustomResolver, IEnumerable<MealPlanRecipe>>(data => data.MealPlanRecipes))
               .ReverseMap()
               .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
