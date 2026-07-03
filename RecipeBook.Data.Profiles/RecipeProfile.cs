using AutoMapper;
using Common.Data.Profiles;
using Common.Models;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles
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
                    opt => opt.MapFrom(src => src.RecipeIngredients == null || src.RecipeIngredients.Count == 0
                        ? new List<RecipeIngredient>()
                        : src.RecipeIngredients
                            .OrderBy(i => i.Product == null || i.Product.ProductCategory == null ? string.Empty : i.Product.ProductCategory.Name)
                            .ThenBy(i => i.Product == null ? string.Empty : i.Product.Name)
                            .ToList())
                )
                .AfterMap((src, dest) => dest.Ingredients?.SetIndexes())
                .ReverseMap()
                .ForMember(dest => dest.RecipeCategory, opt => opt.Ignore())
                .ForMember(
                    dest => dest.RecipeIngredients,
                    opt => opt.MapFrom(src => src.Ingredients == null || src.Ingredients.Count == 0
                        ? new List<RecipeIngredientEditModel>()
                        : src.Ingredients.ToList())
                );
        }
    }
}
