using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class RecipeCategoryProfile : Profile
    {
        public RecipeCategoryProfile()
        {
            CreateMap<RecipeCategory, RecipeCategoryModel>()
               .ReverseMap();

            CreateMap<RecipeCategory, EditRecipeCategoryModel>()
                .ReverseMap();
        }
    }
}
