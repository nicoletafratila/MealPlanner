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
                .IgnoreBaseModelMembers()
                .ReverseMap();

            CreateMap<MealPlan, MealPlanEditModel>()
                .ConstructUsing(_ => new MealPlanEditModel())
                .IgnoreBaseModelMembers()
                .ForMember(
                    m => m.Recipes,
                    o => o.MapFrom<MealPlanToEditMealPlanModelResolver, IList<MealPlanRecipe>?>(s => s.MealPlanRecipes)
                )
                .ReverseMap()
                .ForMember(
                    d => d.MealPlanRecipes,
                    o => o.MapFrom<EditMealPlanModelToMealPlanResolver, IList<RecipeModel>?>(src => src.Recipes)
                );
        }
    }
}