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
            CreateMap<Recipe, RecipeModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    model => model.ImageUrl,
                    opt => opt.MapFrom(data =>
                        $"data:image/jpg;base64,{Convert.ToBase64String(data.ImageContent ?? Array.Empty<byte>())}"
                    ))
                .ForMember(model => model.RecipeCategoryName, opt => opt.MapFrom(data => data.RecipeCategory!.Name))
                .ForMember(model => model.RecipeCategoryId, opt => opt.MapFrom(data => data.RecipeCategory!.Id))
                .ReverseMap()
                .ForMember(dest => dest.RecipeCategory, opt => opt.Ignore());

            CreateMap<Recipe, RecipeEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    model => model.ImageUrl,
                    opt => opt.MapFrom(data =>
                        $"data:image/jpg;base64,{Convert.ToBase64String(data.ImageContent ?? Array.Empty<byte>())}"
                    ))
                .ForMember(
                    model => model.Ingredients,
                    opt => opt.MapFrom<RecipeToEditRecipeModelResolver, IList<RecipeIngredient>?>(data => data.RecipeIngredients!)
                )
                .ReverseMap()
                .ForMember(dest => dest.RecipeCategory, opt => opt.Ignore())
                .ForMember(
                    dest => dest.RecipeIngredients,
                    opt => opt.MapFrom<EditRecipeModelToRecipeResolver, IList<RecipeIngredientEditModel>?>(model => model.Ingredients!)
                );
        }
    }
}