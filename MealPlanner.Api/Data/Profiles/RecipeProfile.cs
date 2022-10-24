using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Data.Profiles
{
    public class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, EditRecipeModel>()
                .ForMember(model => model.ImageContent, opt => opt.Ignore())
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom<RecipeIngredientCustomResolver, IEnumerable<RecipeIngredient>>(data => data.RecipeIngredients))
                .ReverseMap()
                .ForMember(data => data.RecipeIngredients, opt => opt.Ignore())
                .ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<Recipe, RecipeModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ReverseMap()
                .ForMember(data => data.RecipeIngredients, opt => opt.Ignore())
                .ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
