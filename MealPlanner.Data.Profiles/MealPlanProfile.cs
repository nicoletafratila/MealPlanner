using AutoMapper;
using Common.Data.Profiles;
using Common.Models;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Profiles
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
                    o => o.MapFrom(s => s.MealPlanRecipes == null || s.MealPlanRecipes.Count == 0
                        ? new List<Recipe>()
                        : s.MealPlanRecipes
                            .Where(mpr => mpr.Recipe != null)
                            .Select(mpr => mpr.Recipe!)
                            .OrderBy(r => r.RecipeCategory != null ? r.RecipeCategory.DisplaySequence : int.MaxValue)
                            .ThenBy(r => r.Name ?? string.Empty)
                            .ToList())
                )
                .AfterMap((src, dest) => dest.Recipes?.SetIndexes())
                .ReverseMap()
                .ForMember(
                    d => d.MealPlanRecipes,
                    o => o.MapFrom(src => src.Recipes == null || src.Recipes.Count == 0
                        ? new List<MealPlanRecipe>()
                        : src.Recipes
                            .Select(r => new MealPlanRecipe
                            {
                                Id = Guid.NewGuid(),
                                RecipeId = r.Id,
                                MealPlanId = src.Id
                            })
                            .ToList())
                );
        }
    }
}
