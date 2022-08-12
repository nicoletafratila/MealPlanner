using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class RecipeMapper : Profile
    {
        public RecipeMapper()
        {
            CreateMap<Recipe, EditRecipeModel>()
                .ForMember(model => model.ImageContent, opt => opt.Ignore())
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom(data => data.RecipeIngredients.Select(item => item.ToIngredientModel())))
                .ReverseMap()
                .ForMember(model => model.RecipeIngredients, opt => opt.Ignore())
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<Recipe, RecipeModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ReverseMap()
                .ForMember(model => model.RecipeIngredients, opt => opt.Ignore())
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
