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
               .ForMember(model => model.Recipes, opt => opt.MapFrom((data, _, transactionCode, ctx) =>
               {
                   return data.MealPlanRecipes.Select(item => ctx.Mapper.Map<RecipeModel>(item.Recipe));
               }))
               .ReverseMap()
               .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, ShoppingListModel>()
               .ForMember(model => model.Ingredients, opt => opt.MapFrom((data, _, transactionCode, ctx) =>
               {
                   var result = new List<IngredientModel>();
                   foreach (var item in data.MealPlanRecipes)
                   {
                       result.AddRange(ctx.Mapper.Map<EditRecipeModel>(item.Recipe).Ingredients);
                   }
                   return result;
               }))
               .ReverseMap()
               .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
