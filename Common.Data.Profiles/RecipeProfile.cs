using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, EditRecipeModel>()
                .ForMember(model => model.ImageContent, opt => opt.Ignore())
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom<RecipeIngredientCustomResolver, IEnumerable<RecipeIngredient>>(data => data.RecipeIngredients))
                .ForMember(model => model.CategoryId, opt => opt.MapFrom(data => data.Category.Id))
                .ReverseMap();
                //.ForMember(data => data.RecipeIngredients, opt => opt.Ignore())
                //.ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<Recipe, RecipeModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Category, opt => opt.MapFrom(data => data.Category.Name))
                .ForMember(model => model.DisplaySequence, opt => opt.MapFrom(data => data.Category.DisplaySequence))
                .ReverseMap();
                //.ForMember(data => data.RecipeIngredients, opt => opt.Ignore())
                //.ForMember(data => data.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
