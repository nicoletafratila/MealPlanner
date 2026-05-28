using AutoMapper;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;
using Common.Data.Profiles;

namespace RecipeBook.Data.Profiles
{
    public class RecipeCategoryProfile : Profile
    {
        public RecipeCategoryProfile()
        {
            CreateMap<RecipeCategory, RecipeCategoryModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap();

            CreateMap<RecipeCategory, RecipeCategoryEditModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap();
        }
    }
}
