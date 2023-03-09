using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class MealPlanProfile : Profile
    {
        public MealPlanProfile()
        {
            CreateMap<MealPlan, MealPlanModel>()
                .ReverseMap();

            CreateMap<MealPlan, EditMealPlanModel>()
               .ForMember(model => model.Recipes, opt => opt.MapFrom<MealPlanToEditMealPlanModelResolver, IList<MealPlanRecipe>>(data => data.MealPlanRecipes))
               .ReverseMap()
               .ForMember(data => data.MealPlanRecipes, opt => opt.MapFrom<EditMealPlanModelToMealPlanResolver, IList<RecipeModel>>(model => model.Recipes));

            CreateMap<MealPlan, ShoppingListModel>()
               .ForMember(model => model.Ingredients, opt => opt.MapFrom<MealPlanToShoppingListModelResolver, IList<MealPlanRecipe>>(data => data.MealPlanRecipes))
               .ReverseMap();
        }
    }
}
