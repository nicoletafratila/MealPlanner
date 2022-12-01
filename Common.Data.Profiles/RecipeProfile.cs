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
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom<RecipeIngredientCustomResolver, IEnumerable<RecipeIngredient>>(data => data.RecipeIngredients))
                .ReverseMap();

            CreateMap<Recipe, RecipeModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.CategoryName, opt => opt.MapFrom(data => data.RecipeCategory.Name))
                .ForMember(model => model.DisplaySequence, opt => opt.MapFrom(data => data.RecipeCategory.DisplaySequence))
                .ReverseMap();
        }
    }
}
